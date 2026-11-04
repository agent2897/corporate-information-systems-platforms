using System.Data;

namespace ProjectContracts;

public class MainForm : Form
{
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
    private readonly ComboBox projectFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly TextBox searchBox = new() { Width = 260, PlaceholderText = "Прізвище або його початок" };
    private readonly DataGridView participationGrid = CreateGrid();
    private readonly DataGridView searchGrid = CreateGrid();
    private readonly StatusStrip status = new();
    private readonly ToolStripStatusLabel statusLabel = new();

    public MainForm()
    {
        Text = "Проектні договори — ADO.NET";
        Size = new Size(1300, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        AddGridTab("Працівники", Queries.Employees);
        AddGridTab("Посади та оклади", Queries.Positions);
        AddGridTab("Проекти", Queries.Projects);
        AddParticipationTab();
        AddGridTab("У двох проектах", Queries.TwoProjects);
        AddSearchTab();

        status.Items.Add(statusLabel);
        Controls.Add(tabs);
        Controls.Add(status);
        UpdateStatus();
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        BackgroundColor = Color.White,
        AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(235, 242, 252) },
        ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) },
        EnableHeadersVisualStyles = false,
    };

    private void AddGridTab(string title, string sql)
    {
        var grid = CreateGrid();
        grid.DataSource = Db.Query(sql);
        var page = new TabPage(title);
        page.Controls.Add(grid);
        tabs.TabPages.Add(page);
    }

    private void AddParticipationTab()
    {
        var projects = Db.Query(Queries.ProjectList);
        var all = projects.NewRow();
        all["Id"] = 0L; all["Name"] = "Усі проекти";
        projects.Rows.InsertAt(all, 0);
        projectFilter.DisplayMember = "Name";
        projectFilter.ValueMember = "Id";
        projectFilter.DataSource = projects;
        projectFilter.SelectedIndexChanged += (_, _) => LoadParticipation();

        var page = new TabPage("Участь у проектах");
        page.Controls.Add(participationGrid);
        page.Controls.Add(Toolbar(new Label { Text = "Проект:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, projectFilter));
        tabs.TabPages.Add(page);
        LoadParticipation();
    }

    private void LoadParticipation()
    {
        var id = projectFilter.SelectedValue is long v ? v : 0L;
        participationGrid.DataSource = Db.Query(Queries.Participation, ("@projectId", id));
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
        searchGrid.DataSource = Db.Query(Queries.Search, ("@pattern", text.Trim() + "%"));
    }

    public void SelectProject(int index) => projectFilter.SelectedIndex = index;

    private static FlowLayoutPanel Toolbar(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6) };
        panel.Controls.AddRange(controls);
        return panel;
    }

    private void UpdateStatus()
    {
        statusLabel.Text =
            $"Працівників: {Db.Scalar("SELECT COUNT(*) FROM Employees")}   |   " +
            $"Посад: {Db.Scalar("SELECT COUNT(*) FROM Positions")}   |   " +
            $"Проектів: {Db.Scalar("SELECT COUNT(*) FROM Projects")}   |   " +
            $"Фонд окладів: {Convert.ToDouble(Db.Scalar("SELECT SUM(p.Salary) FROM Employees e JOIN Positions p ON p.Id = e.PositionId")):N2} грн";
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
        tabs.SelectedIndex = 3; SelectProject(0); Shot("4_participation.png");
        SelectProject(1); Shot("5_participation_filter.png");
        tabs.SelectedIndex = 4; Shot("6_two_projects.png");
        tabs.SelectedIndex = 5; Search("К"); Shot("7_search.png");
    }
}
