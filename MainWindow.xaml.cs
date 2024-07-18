using CBakWeChatDesktop.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CBakWeChatDesktop
{
    public partial class MainWindow : Window
    {
        MainData data = new MainData();
        public List<Student> Students { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // 数据绑定
            bindData();
            LoadStudents();
            StudentList.ItemsSource = Students;
        }

        private void bindData()
        {
            data.server = Properties.Settings.Default.server;
            data.token = Properties.Settings.Default.token;
            data.isLogin = Properties.Settings.Default.isLogin;
            DataContext = data;
        }

        private void LoadStudents()
        {
            Students = new List<Student>
            {
                new Student { Name = "John Doe", Age = 20, Grade = "A", Address = "123 Main St" },
                new Student { Name = "Jane Smith", Age = 22, Grade = "B", Address = "456 Oak St" },
                new Student { Name = "Sam Johnson", Age = 19, Grade = "A", Address = "789 Pine St" }
            };
        }

        private void StudentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentList.SelectedItem is Student selectedStudent)
            {
                StudentName.Text = selectedStudent.Name;
                StudentAge.Text = selectedStudent.Age.ToString();
                StudentGrade.Text = selectedStudent.Grade;
                StudentAddress.Text = selectedStudent.Address;
            }
            else
            {
                StudentName.Text = "";
                StudentAge.Text = "";
                StudentGrade.Text = "";
                StudentAddress.Text = "";
            }
        }

        private void SessionAddClick(object sender, RoutedEventArgs e)
        {
            var msg = WeChatMsgScan.ReadProcess();
            MessageBox.Show(msg.accountname);
            AddSessionWindow addSessionWindow = new AddSessionWindow(this, msg);
            addSessionWindow.ShowDialog();
        }
    }

    public class MainData
    {
        public string server { get; set; }
        public string token { get; set; }
        public bool isLogin { get; set; }
    }

    public class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Grade { get; set; }
        public string Address { get; set; }
    }

}
