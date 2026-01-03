// See https://aka.ms/new-console-template for more information

using SopProj.ChildTaskWork;

string? command;
var list = new Dictionary<(string, string), Task>();
while (true)
{
    command = Console.ReadLine();
    if (command == null) continue;
    string[] words = command.Split(' ');
    if (words[0] == "add")
    {
        if (words.Length < 3)
        {
            Console.Error.WriteLine("Invalid command");
        }
        else
        {
            string sourcePath =  words[1];
            for (int i = 2; i < words.Length; i++)
            {
                string targetPath = words[i];
                if (list.ContainsKey((sourcePath, targetPath)))
                {
                    Console.Error.WriteLine("Duplicate source path: " + sourcePath + " and target path: " + targetPath
                    + "\nCopy skipped");
                    continue;
                }
                //list[(sourcePath, targetPath)] = 
                    await Task.Run(() =>
                {
                    CopingMethods.CopyAll(sourcePath, targetPath);
                    var watcher = new Monitoring(sourcePath, targetPath);
                    watcher.Watch();
                });
            }
        }
    }
    else if (words[0] == "end")
    {
        //to implement
    }
    else if (words[0] == "list")
    {
        //to implement
    }
    else if (words[0] == "restore")
    {
        //to implement
    }
}