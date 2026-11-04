namespace ProjectContractsEf;

public class MainForm : Form
{
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
    private readonly ComboBox projectFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly TextBox searchBox = new() { Width = 260, PlaceholderText = "Прізвище або його початок" };
    private readonly DataGridView participationGrid = CreateGrid();
    private readonly DataGridView searchGrid = CreateGrid();
    private readonly StatusStrip status = new();

    public MainForm()
    {
        Text = "Проектні договори — Entity Framework Core";
        Size = new Size(1300, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        AddGridTab("Працівники", Repository.Employees());
        AddGridTab("Посади та оклади", Repository.Positions());
        AddGridTab("Проекти", Repository.Projects());
        AddParticipationTab();
        AddGridTab("У двох проектах", Repository.TwoProjects());
        AddSearchTab();

        status.Items.Add(new ToolStripStatusLabel(Repository.Summary()));
        Controls.Add(tabs);
        Controls.Add(status);
    }

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(232, 245, 233) },
            ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) },
            EnableHeadersVisualStyles = false,
        };
        // Назви властивостей проекцій: "Оклад_грн" -> "Оклад, грн", "Домашня_адреса" -> "Домашня адреса"
        grid.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.HeaderText = col.Name == "Код" ? "№" : col.Name.Replace("_грн", ", грн").Replace("_дн", ", дн.").Replace('_', ' ');
        };
        return grid;
    }

    private void AddGridTab(string title, IList<object> data)
    {
        var grid = CreateGrid();
        grid.DataSource = data;
        var page = new TabPage(title);
        page.Controls.Add(grid);
        tabs.TabPages.Add(page);
    }

    private void AddParticipationTab()
    {
        projectFilter.Items.Add(new ProjectItem(0, "Усі проекти"));
        foreach (var (id, name) in Repository.ProjectList())
            projectFilter.Items.Add(new ProjectItem(id, name));
        projectFilter.SelectedIndexChanged += (_, _) =>
            participationGrid.DataSource = Repository.Participation(((ProjectItem)projectFilter.SelectedItem!).Id);

        var page = new TabPage("Участь у проектах");
        page.Controls.Add(participationGrid);
        page.Controls.Add(Toolbar(new Label { Text = "Проект:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, projectFilter));
        tabs.TabPages.Add(page);
        projectFilter.SelectedIndex = 0;
    }

    private record ProjectItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }

    private void AddSearchTab()
    {
        var button = new Button { Text = "Знайти", AutoSize = true };
        button.Click += (_, _) => Search(searchBox.Text);
        searchBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Search(searchBox.Text); };

        var page = new TabPage("Пошук працівника");
        page.Controls.Add(searchGrid);
        page.Controls.Add(Toolbar(new Label { Text = "Прізвище:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, searchBox, button));
        tabs.TabPages.Add(page);
        Search("");
    }

    public void Search(string text)
    {
        searchBox.Text = text;
        searchGrid.DataSource = Repository.Search(text);
    }

    private static FlowLayoutPanel Toolbar(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6) };
        panel.Controls.AddRange(controls);
        return panel;
    }

    /// <summary>Режим знімків екрана для звіту: --shots &lt;папка&gt;.</summary>
    public void SaveScreenshots(string dir)
    {
        Directory.CreateDirectory(dir);
        void Shot(string name)
        {
            Application.DoEvents();
            using var bmp = new Bitmap(Width, Height);
            DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
            bmp.Save(Path.Combine(dir, name), System.Drawing.Imaging.ImageFormat.Png);
        }
        tabs.SelectedIndex = 0; Shot("1_employees.png");
        tabs.SelectedIndex = 1; Shot("2_positions.png");
        tabs.SelectedIndex = 2; Shot("3_projects.png");
        tabs.SelectedIndex = 3; projectFilter.SelectedIndex = 0; Shot("4_participation.png");
        projectFilter.SelectedIndex = 2; Shot("5_participation_filter.png");
        tabs.SelectedIndex = 4; Shot("6_two_projects.png");
        tabs.SelectedIndex = 5; Search("К"); Shot("7_search.png");
    }
}
