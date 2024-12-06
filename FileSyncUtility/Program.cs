using FileSyncUtility.Infrastructure;

var prc = new SynchronizationProcess();
prc.ProcessNotification += (sender, e) =>
{
    Console.WriteLine(e.Message);
};
prc.Start("C:\\logs\\Master", "C:\\logs\\Test");

Console.ReadLine();