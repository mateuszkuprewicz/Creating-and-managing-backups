// See https://aka.ms/new-console-template for more information

using SopProj.ChildTaskWork;

string? command;
var list = new Dictionary<(string, string), (Task, CancellationTokenSource)>();
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
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                list[(sourcePath, targetPath)] = 
                    (Task.Run(() =>
                    {
                        CopingMethods.CopyAll(sourcePath, targetPath);
                        var watcher = new Monitoring(sourcePath, targetPath);
                        watcher.Watch(tokenSource.Token);
                        list.Remove((sourcePath, targetPath)); //Task leaves watcher only when canceled
                    }, tokenSource.Token), tokenSource);
            }
        }
    }
    else if (words[0] == "end")
    {
        if(words.Length < 3)
            Console.Error.WriteLine("Invalid command");
        else
        {
            string sourcePath = words[1];
            for (int i = 2; i < words.Length; i++)
            {
                string targetPath = words[i];
                if (!list.Keys.Select(x => x.Item1).Contains(sourcePath))
                {
                    Console.Error.WriteLine("Invalid source path: " + sourcePath);
                    break;
                }
                else if (list.ContainsKey((sourcePath, targetPath)))
                {
                    list[(sourcePath, targetPath)].Item2.Cancel();
                    list.Remove((sourcePath, targetPath));
                }
                else
                {
                    Console.Error.WriteLine("Invalid target path: " + targetPath);
                }
            }
        }
    }
    else if (words[0] == "list")
    {
        if (words.Length != 1)
        {
            Console.Error.WriteLine("Invalid command");
        }
        else
        {
            foreach (var pair in list.Keys)
            {
                Console.WriteLine(pair.Item1 + "-> " + pair.Item2);
            }
        }
    }
    else if (words[0] == "exit")
    {
        if (words.Length != 1)
        {
            Console.Error.WriteLine("Invalid command");
        }
        else
        {
            foreach (var cancellationTokenSource in list.Values.Select(x => x.Item2))
            {
                cancellationTokenSource.Cancel();
            }
            Environment.Exit(0);
        }
    }
    else if (words[0] == "restore")
    {
        if(words.Length != 3)
            Console.Error.WriteLine("Invalid command");
        else
        {
            string sourcePath = words[1];
            string targetPath = words[2];
            if(!list.Keys.Select(x => x.Item1).Contains(sourcePath))
                Console.Error.WriteLine("Invalid source path: " + sourcePath);
            else if (!list.Keys.Select(x => x.Item2).Contains(targetPath))
                Console.Error.WriteLine("Invalid target path: " + targetPath);
            else
            {
                list[(sourcePath, targetPath)].Item2.Cancel();
                CopingMethods.CopyAll(targetPath, sourcePath); //blocking the thread as the project description demands
                Console.WriteLine($"Restored {sourcePath} from {targetPath}.");
                CancellationTokenSource cts = new CancellationTokenSource();
                list.Remove((sourcePath, targetPath));
                list[(sourcePath, targetPath)] = 
                    (Task.Run(() =>
                    {
                        var watcher = new Monitoring(sourcePath, targetPath);
                        watcher.Watch(cts.Token);
                        list.Remove((sourcePath, targetPath)); //Task leaves watcher only when canceled
                    }, cts.Token), cts);
                Console.WriteLine($"Monitoring {sourcePath} -> {targetPath} resumed");
            }
        }
    }
}