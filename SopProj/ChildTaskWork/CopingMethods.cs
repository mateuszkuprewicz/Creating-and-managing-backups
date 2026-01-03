namespace SopProj.ChildTaskWork;

public class CopingMethods
{
    public static int CopyAll(string sourcePath, string destinationPath)
    {
        //is dest inside source?
        if (IsNested(sourcePath, destinationPath))
        {
            Console.Error.WriteLine("Cannot make a Copy inside a copied directory");
            return -2;
        }
        //Does source folder exist?
        if (!Directory.Exists(sourcePath))
        {
            Console.Error.WriteLine("Source directory not found");
            return -1;
        }
        //Is destination Folder correct
        //Does dir exist?
        if(!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);
        //Is dir empty?
        if (Directory.GetFileSystemEntries(destinationPath).Length > 0)
        {
            var dir = new DirectoryInfo(destinationPath);
            dir.Delete(true);
            if (Directory.Exists(destinationPath))
                throw new FileNotFoundException();
            Directory.CreateDirectory(destinationPath);
        }
        //Coping directories
        foreach (var dir in GetDirectoriesNoSymlinks(sourcePath))
        {
            Directory.CreateDirectory(GetDestPath(sourcePath, destinationPath,dir));
        }
        //Coping files 
        foreach (var file in GetFileNoDirSymlinks(sourcePath))
        {
            var fileInfo = new FileInfo(file);
            if(fileInfo.LinkTarget == null)
                File.Copy(file, GetDestPath(sourcePath, destinationPath, file));
        }
        //Coping symlinks
        ResursiveCopySymlink(sourcePath, sourcePath, destinationPath);
        return 0;
    }

    static bool IsNested(string sourcePath, string destinationPath)
    {
        DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
        DirectoryInfo destinationDir = new DirectoryInfo(destinationPath);
        if (sourceDir.FullName == destinationDir.FullName)
            return true;
        while (destinationDir.Parent != null)
        {
            if (destinationDir.Parent.FullName == sourceDir.FullName) return true;
            destinationDir = destinationDir.Parent;
        }

        return false;
    }

    public static void CopySymLink(string sourcePath, string destinationPath, string file)
    {
        var fileInfo = new FileInfo(file) as FileSystemInfo;
        if(fileInfo.LinkTarget == null)
            throw new FileNotFoundException();
        if (Directory.GetFileSystemEntries(sourcePath,"*",SearchOption.AllDirectories)
            .Contains(fileInfo.LinkTarget)) //if symlonk points to file/dir inside source dir
        {
            string newSymLinkTarget = Path.GetFullPath(GetDestPath(sourcePath, destinationPath, fileInfo.LinkTarget));
            string newSymLinkPath = GetDestPath(sourcePath, destinationPath, file);
            if(File.Exists(fileInfo.LinkTarget))
                File.CreateSymbolicLink(newSymLinkPath, newSymLinkTarget);
            else if(Directory.Exists(fileInfo.LinkTarget))
                Directory.CreateSymbolicLink(newSymLinkPath, newSymLinkTarget);
        }
        else //if symlink points to file/dir outside source dir
        {
            string newSymLinkTarget = Path.GetFullPath(fileInfo.LinkTarget);
            string newSymLinkPath = GetDestPath(sourcePath, destinationPath, file);
            File.CreateSymbolicLink(newSymLinkPath, newSymLinkTarget);
        }
    }

    public static string GetDestPath(string sourcePath, string destinationPath, string file)
    {
        sourcePath = Path.GetFullPath(sourcePath);
        destinationPath = Path.GetFullPath(destinationPath);
        string relDest = Path.GetRelativePath(sourcePath, file);
        return Path.Combine(destinationPath, relDest);
    }
    
    public static IEnumerable<string> GetDirectoriesNoSymlinks(string path)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            var info = new DirectoryInfo(dir);

            if (info.LinkTarget != null)
                continue;

            yield return dir;

            foreach (var sub in GetDirectoriesNoSymlinks(dir))
                yield return sub;
        }
    }

    public static IEnumerable<string> GetFileNoDirSymlinks(string path)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            yield return file;
        }

        foreach (var dir in Directory.GetDirectories(path))
        {
            var info = new DirectoryInfo(dir);

            if (info.LinkTarget != null)
                continue;

            foreach (var file in GetFileNoDirSymlinks(dir))
                yield return file;
        }
    }

    public static void ResursiveCopySymlink(string sourcePath, string curDir, string destinationPath)
    {
        foreach (var entry in Directory.GetFileSystemEntries(curDir))
        {
            FileSystemInfo info = Directory.Exists(entry) ? new DirectoryInfo(entry) : new FileInfo(entry);
            if(info.LinkTarget != null)
                CopySymLink(sourcePath, destinationPath, entry);
            else if(Directory.Exists(entry)) 
                ResursiveCopySymlink(sourcePath, entry, destinationPath);
        }
    }
}