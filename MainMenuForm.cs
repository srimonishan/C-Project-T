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
        private readonly Color _sidebarColor = Color.FromArgb(45, 38, 33);
        private readonly Color _activeColor = Color.FromArgb(196, 122, 56);
        private Button _activeNavButton;

        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {
            if (!ConnectionTest.TestConnection())
            {
                MessageBox.Show(
                    "Could not connect to the database. Some features may not work correctly.",
                    "Database Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            SetActiveButton(btnCraftItems);
            lblPageTitle.Text = "Craft Items";
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            var clicked = sender as Button;
            if (clicked == null) return;

            SetActiveButton(clicked);
            lblPageTitle.Text = clicked.Text.Trim();
            if(clicked==btnArtisans)
            {
                var f=new LocalArtisanCraftMarket.Forms.ArtisanSignupForm();
                f.ShowDialog(this);
            }
        }

        private void SetActiveButton(Button button)
        {
            if (_activeNavButton != null)
            {
                _activeNavButton.BackColor = _sidebarColor;
            }

            button.BackColor = _activeColor;
            _activeNavButton = button;
        }

        /// <summary>
        /// Swaps the content panel to display the given control, replacing whatever is currently shown.
        /// </summary>
        private void LoadView(Control view)
        {
            pnlContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
        }
    }
}