using System.Drawing;
using System.Windows.Forms;

public partial class QRVentana : Form
{
    private PictureBox pbQRCodeDisplay;
    private Button btnSaveQR; 

    public QRVentana(Bitmap qrCodeImage)
    {
        InitializeComponent(); 

        this.Text = "Código QR del Vehículo"; 
        this.Size = new Size(300, 350); 
        this.FormBorderStyle = FormBorderStyle.FixedDialog; 
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent; 

        pbQRCodeDisplay = new PictureBox()
        {
            Location = new Point(25, 20), 
            Size = new Size(250, 250),   
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom, 
            Image = qrCodeImage 
        };
        this.Controls.Add(pbQRCodeDisplay);

        btnSaveQR = new Button()
        {
            Text = "Guardar QR",
            Location = new Point(100, 280), 
            Size = new Size(100, 30)
        };
        btnSaveQR.Click += BtnSaveQR_Click;
        this.Controls.Add(btnSaveQR);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
     
        this.ClientSize = new System.Drawing.Size(284, 261); 
        this.Name = "QRVentana";
        this.ResumeLayout(false);
    }

    private void BtnSaveQR_Click(object sender, System.EventArgs e)
    {
        if (pbQRCodeDisplay.Image != null)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png|Bitmap Image|*.bmp|JPEG Image|*.jpg";
                sfd.Title = "Guardar Código QR como Imagen";
                sfd.FileName = "qrcode_vehiculo.png"; 

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    pbQRCodeDisplay.Image.Save(sfd.FileName);
                    MessageBox.Show("QR Code guardado exitosamente.", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        if (pbQRCodeDisplay.Image != null)
        {
            pbQRCodeDisplay.Image.Dispose();
            pbQRCodeDisplay.Image = null;
        }
    }
}