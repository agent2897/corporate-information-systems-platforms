namespace ProjectContractsEf;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        using (var db = new ContractsContext())
            db.Database.EnsureCreated();   // створює схему з моделі та наповнює даними HasData

        var form = new MainForm();
        if (args.Length == 2 && args[0] == "--shots")
            form.Shown += (_, _) => { form.SaveScreenshots(args[1]); form.Close(); };
        Application.Run(form);
    }
}
