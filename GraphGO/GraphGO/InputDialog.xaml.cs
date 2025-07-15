using System.Linq;
using System.Windows;

namespace GraphDemo
{
    public partial class InputDialog : Window
    {
        public int ResponseText { get; set; }

        public InputDialog(string defaultText = "")
        {
            InitializeComponent();
            tbWeight.Text = defaultText;
            tbWeight.Focus();
            tbWeight.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbWeight.Text, out int newWeight))
            {
                ResponseText = newWeight;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректный вес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void InputTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            string input = e.Text;
           
            e.Handled = !input.All(char.IsDigit);
            
        }
    }
}
