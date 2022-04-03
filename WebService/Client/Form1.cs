using System;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }         
        private void btnSend_Click(object sender, EventArgs e)
        {
            CarInfoServiceReference.CarInfo carInfo = new CarInfoServiceReference.CarInfo();
            carInfo.CarPrice = Convert.ToDouble(tbPrice.Text);
            carInfo.CarDescription = tbDescription.Text;
            carInfo.CarMaxSpeed = Convert.ToDouble(tbMaxSpeed.Text);

            CarInfoServiceReference.CarInfo carInfoTranslated = new CarInfoServiceReference.CarInfo();

            CarInfoServiceReference.CarInfoServiceSoapClient service = new CarInfoServiceReference.CarInfoServiceSoapClient();
            carInfoTranslated = service.Translate(carInfo);

            Form2 form = new Form2();
            form.tbDescription.Text = carInfoTranslated.CarDescription;
            form.tbMaxSpeed.Text = carInfoTranslated.CarMaxSpeed.ToString();
            form.tbPrice.Text = carInfoTranslated.CarPrice.ToString();
            form.Show();
        }
    }
}
