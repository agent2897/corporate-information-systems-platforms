namespace ProjectContracts;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Db.EnsureCreated();
        var form = new MainForm();

        if (args.Length == 2 && args[0] == "--shots")
        {
            form.Shown += (_, _) => { form.SaveScreenshots(args[1]); form.Close(); };
        }
        Application.Run(form);
    }
}
