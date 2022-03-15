namespace LunaEdit
{
    public partial class OptionFrm : Form
    {
        public object option;

        public OptionFrm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OptionFrm_Load(object sender, EventArgs e)
        {
            this.propertyGrid1.SelectedObject = option;
        }
    }
}
