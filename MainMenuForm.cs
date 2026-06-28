using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalArtisanCraftMarket.Database;

namespace LocalArtisanCraftMarket
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {
            if (ConnectionTest.TestConnection())
            {
                MessageBox.Show("Database Connected Successfully!");
            }
            else
            {
                MessageBox.Show("Database Connection Failed!");
            }
        }
    }
}
