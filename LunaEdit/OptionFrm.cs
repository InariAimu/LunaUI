namespace LunaEdit;

public partial class OptionFrm : Form
{
    public object? option;

    public OptionFrm()
    {
        InitializeComponent();
    }

    private void OptionFrm_Load(object sender, EventArgs e)
    {
        if (option is not null)
            propertyGrid1.SelectedObject = option;
    }
}
