namespace SopProj.ChildTaskWork;

public class Monitoring
{
    public Monitoring(string sourcePath, string destinationPath)
    {
        SourcePath = sourcePath;
        TargetPath = destinationPath;
    }
    private string TargetPath { get; set; }
    private string SourcePath { get; set; }
    
    public void Watch()
    {
        using var watcher = new FileSystemWatcher(SourcePath, "*");
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        
        Thread.Sleep(Timeout.Infinite);
    }
    
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }

        if (Directory.Exists(e.FullPath)) //dir
        {
            return;
        }
        else //file
        {
            FileInfo entry  = new FileInfo(e.FullPath);
            if (entry.LinkTarget == null) // not a symbolic link
            {
                string targetPath = CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath);

                File.Copy(e.FullPath, targetPath, true);
            }
            else //symbolic link
            {
                CopingMethods.CopySymLink(SourcePath, TargetPath, e.FullPath);
            }
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        // await Task.Delay(100);
        // if (!Directory.Exists(e.FullPath) && !File.Exists(e.FullPath)) return;
        
        if (Directory.Exists(e.FullPath)) //dir
        {
            Directory.CreateDirectory(CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath));
        }
        else //file
        {
            FileInfo entry = new FileInfo(e.FullPath);
            if (entry.LinkTarget == null) //not a symbolic link
            {
                string targetPath = CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath);
                File.Copy(e.FullPath, targetPath, true);
            }
            else //symbolic link
            {
                CopingMethods.CopySymLink(SourcePath, TargetPath, e.FullPath);
            }
        }
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (Directory.Exists(e.FullPath)) //dir
        {
            Directory.Delete(CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath), true);
        }
        else //file
        {
            File.Delete(CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath));
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if(e.OldFullPath == e.FullPath)
            return;
        if (Directory.Exists(e.FullPath)) //dir
        {
            Directory.Move(CopingMethods.GetDestPath(SourcePath, TargetPath, e.OldFullPath ),
                CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath));
        }
        else //file
        {
            File.Move(CopingMethods.GetDestPath(SourcePath, TargetPath, e.OldFullPath),
                CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath), true);
        }
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine(e.GetException());
    }
}