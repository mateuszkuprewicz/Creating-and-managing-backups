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
    public void Watch(CancellationToken token)
    {
        using var watcher = new FileSystemWatcher(SourcePath, "*");
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        while (Directory.Exists(SourcePath) && !token.IsCancellationRequested)
        {
            Thread.Sleep(100);
        }
        if (token.IsCancellationRequested)
            Console.WriteLine($"Monitoring {SourcePath} to {TargetPath} ended");
        else
        {
            Console.Error.WriteLine($"{SourcePath} missing. Monitoring {SourcePath} -> {TargetPath} ended");
        }
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
        if (Directory.Exists(e.FullPath)) //dir
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(e.FullPath);
            if (directoryInfo.LinkTarget == null) //normal directory
            {
                Directory.CreateDirectory(CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath));
            }
            else //symbolic link
            {
                CopingMethods.CopySymLink(SourcePath, TargetPath, e.FullPath);
            }
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
        string destPath = CopingMethods.GetDestPath(SourcePath, TargetPath, e.FullPath); //Entry in the source-dir 
        //doesn't exist, so the type of entry must be checked in the backup directory 
        if (Directory.Exists(destPath)) //dir
        {
            Directory.Delete(destPath, true);
        }
        else //file
        {
            File.Delete(destPath);
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

    private void OnError(object sender, ErrorEventArgs e)
    {
        Console.Error.WriteLine(e.GetException());
    }
}