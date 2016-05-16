using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data;
using System.Linq;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityProjectManager
{ 
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        RegistryKey Unity4X;
        RegistryKey Unity5X;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegistryKey UnityTechnologies = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).OpenSubKey("Unity Technologies", true);
            Unity4X = UnityTechnologies.OpenSubKey("Unity Editor 4.x", true);
            Unity5X = UnityTechnologies.OpenSubKey("Unity Editor 5.x", true);  
             ///
            RefreshDataGrid();
        }

        void RefreshDataGrid()
        {
            dataGrid.ItemsSource = null;
            dt = new DataTable();
            dt.Columns.Add(this.projectVersion, System.Type.GetType("System.String")); ///100
            dt.Columns.Add(this.projectName, System.Type.GetType("System.String"));///80
            dt.Columns.Add(this.projectPath, System.Type.GetType("System.String"));///300
            dt.Columns.Add(this.projectID, System.Type.GetType("System.String"));///300

            foreach (var item in Unity4X.GetValueNames())
            {
                if (item.IndexOf(registryKeyProjectName) >= 0)
                {
                    //MessageBox.Show(Unity4X.GetValue(item).ToString());

                    string projectPath = Unity4X.GetValue(item).ToString();
                    string[] projectNames = projectPath.Split('/');
                    CreateRows(unity4X, projectNames[projectNames.Length - 1], projectPath, item.ToString());
                }
            }

            foreach (var item in Unity5X.GetValueNames())
            {
                if (item.IndexOf(registryKeyProjectName) >= 0)
                {
                    //MessageBox.Show(Unity4X.GetValue(item).ToString());
                    byte[] str = ObjectToBytes(Unity5X.GetValue(item));
                    str = byteCut(str, 0x00);
                    string projectPath = Encoding.Default.GetString(str).Trim();
                    int index = 10;
                    projectPath = projectPath.Substring(index, projectPath.Length - index);
                    string[] projectNames = projectPath.Split('/');
                    CreateRows(unity5X, projectNames[projectNames.Length - 1], projectPath, item.ToString());
                }
            }

            dataGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
            dataGrid.SelectionMode = DataGridSelectionMode.Extended;

            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = dt.DefaultView;
        }

        public static byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        byte[] byteCut(byte[] b, byte cut)
        {
            List<byte> list = new List<byte>(b);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == cut)
                    list.Remove(list[i]);
            }
            return list.ToArray();
        }

    DataTable dt;
        string registryKeyProjectName = "RecentlyUsedProjectPaths-";
        string projectVersion = "Unity版本";
        string projectName = "项目名称";
        string projectPath = "UnityProjcet详细路径";
        string projectID = "项目ID";
        string unity4X = "Unity4.X";
        string unity5X = "Unity5.X";

        void CreateRows(string projectVersion, string projectName, string projectPath, string projectID)
        {
            DataRow dr = dt.NewRow();
            dr[this.projectVersion] = projectVersion;
            dr[this.projectName] = projectName;
            dr[this.projectPath] = projectPath;
            dr[this.projectID] = projectID;
            dt.Rows.Add(dr);
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //MessageBox.Show(dataGrid.SelectedItems.Count.ToString());
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItems.Count <= 0)
            {
                MessageBox.Show("请选中要删除的选项！！！","删除框");
                return;
            }

            List<string> projectids = new List<string>();
            List<string> projectversions = new List<string>();
            foreach (var item in dataGrid.SelectedItems)
            {
                DataRowView drv = item as DataRowView;
                projectids.Add(drv[this.projectID].ToString());
                projectversions.Add(drv[this.projectVersion].ToString());
            }

            if (MessageBox.Show("是否删除选中的" + projectversions.Count + "项", "删除框", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < projectversions.Count; i++)
                {
                    string version = projectversions[i];
                    string id = projectids[i];
                    if (version == unity4X)
                    {
                        if (Unity4X.GetValue(id) != null)
                        {
                            Unity4X.DeleteValue(id);
                        }
                    }
                    else if(version == unity5X)
                    {
                        if (Unity5X.GetValue(id) != null)
                        {
                            Unity5X.DeleteValue(id);
                        }
                    }
                }
            }

            RefreshDataGrid();
        }

        private void delAll_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.Items.Count <= 0)
            {
                MessageBox.Show("无可删除选项！！！","删除框");
                return;
            }

            if (MessageBox.Show("是否删除" + dataGrid.Items.Count + "项", "删除框", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (var item in dataGrid.Items)
                {
                    DataRowView drv = item as DataRowView;
                    string version = drv[this.projectVersion].ToString();
                    string id = drv[this.projectID].ToString();
                    if (version == unity4X)
                    {
                        if (Unity4X.GetValue(id) != null)
                        {
                            Unity4X.DeleteValue(id);
                        }
                    }
                    else if (version == unity5X)
                    {
                        if (Unity5X.GetValue(id) != null)
                        {
                            Unity5X.DeleteValue(id);
                        }
                    }
                }
            }

            RefreshDataGrid();
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
        }
    }
}
