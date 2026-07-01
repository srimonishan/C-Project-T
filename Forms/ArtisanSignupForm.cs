using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LocalArtisanCraftMarket.Models;
using LocalArtisanCraftMarket.Services;

namespace LocalArtisanCraftMarket.Forms
{
    public class ArtisanSignupForm:Form
    {
        private TextBox txtName,txtEmail,txtPhone,txtAddress,txtPassword;
        private Button btnSignup,btnCancel;
        private ErrorProvider err=new ErrorProvider();
        private ArtisanService _service=new ArtisanService();
        public ArtisanSignupForm()
        {
            Init();
        }
        private void Init()
        {
            Font=factoryFont();
            var lblTitle=new Label{Text="Artisan Sign Up",AutoSize=true,Font=new Font(Font.FontFamily,14,FontStyle.Bold),Left=20,Top=10};
            var y=50;
            txtName=new TextBox{Left=140,Top=y,Width=300};
            var l1=new Label{Text="Full name",Left=20,Top=y+3,AutoSize=true};
            y+=40;
            txtEmail=new TextBox{Left=140,Top=y,Width=300};
            var l2=new Label{Text="Email",Left=20,Top=y+3,AutoSize=true};
            y+=40;
            txtPhone=new TextBox{Left=140,Top=y,Width=300};
            var l3=new Label{Text="Phone",Left=20,Top=y+3,AutoSize=true};
            y+=40;
            txtAddress=new TextBox{Left=140,Top=y,Width=300};
            var l4=new Label{Text="Address",Left=20,Top=y+3,AutoSize=true};
            y+=40;
            txtPassword=new TextBox{Left=140,Top=y,Width=300,UseSystemPasswordChar=true};
            var l5=new Label{Text="Password",Left=20,Top=y+3,AutoSize=true};
            y+=54;
            btnSignup=new Button{Left=140,Top=y,Width=140,Text="Sign Up"};
            btnCancel=new Button{Left=300,Top=y,Width=140,Text="Cancel"};
            btnSignup.Click+=OnSignup;
            btnCancel.Click+=(_,__)=>DialogResult=DialogResult.Cancel;
            AcceptButton=btnSignup;CancelButton=btnCancel;
            Controls.AddRange(new Control[]{lblTitle,l1,txtName,l2,txtEmail,l3,txtPhone,l4,txtAddress,l5,txtPassword,btnSignup,btnCancel});
            StartPosition=FormStartPosition.CenterParent;ClientSize=new Size(480, y+90);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;Text="Artisan Sign Up";
        }
        private Font factoryFont(){return new Font("Segoe UI",9f);}
        private void OnSignup(object s,EventArgs e)
        {
            err.Clear();
            if(string.IsNullOrWhiteSpace(txtName.Text)){err.SetError(txtName,"Required");return;}
            if(string.IsNullOrWhiteSpace(txtEmail.Text)||!Regex.IsMatch(txtEmail.Text.Trim(),"^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")){err.SetError(txtEmail,"Invalid");return;}
            if(string.IsNullOrWhiteSpace(txtPassword.Text)||txtPassword.Text.Length<6){err.SetError(txtPassword,"Min 6 chars");return;}
            var artisan=new Artisan{FullName=txtName.Text.Trim(),Email=txtEmail.Text.Trim(),Phone=txtPhone.Text.Trim(),Address=txtAddress.Text.Trim(),Password=txtPassword.Text};
            var r=_service.Register(artisan);
            if(!r.Success){MessageBox.Show(r.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}
            MessageBox.Show("Signup successful","",MessageBoxButtons.OK,MessageBoxIcon.Information);
            DialogResult=DialogResult.OK;
        }
    }
}
