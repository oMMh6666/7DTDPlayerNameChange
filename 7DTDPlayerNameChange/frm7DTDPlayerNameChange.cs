using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace _7DTDPlayerNameChange
{
    public partial class frm7DTDPlayerNameChange : Form
    {
        private const string RegistryPath = @"HKEY_CURRENT_USER\SOFTWARE\The Fun Pimps\7 Days To Die";
        private const string RegistryKeyName = "PlayerName_h775476977";

        private string CurrentValue = string.Empty;

        public frm7DTDPlayerNameChange()
        {
            InitializeComponent();
        }

        private void frm7DTDPlayerNameChange_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new ThreadStart(Init));
        }

        private void Init()
        {
            LoadUI();
        }

        private void LoadUI()
        {
            if (RegistryHelper.RegistryKeyExists(RegistryPath, RegistryKeyName))
            {
                CurrentValue = RegistryHelper.ConvertRegistryBinaryToString(RegistryPath, RegistryKeyName);
                txtPlayerName.Text = CurrentValue;
                btnOK.Enabled = false;

                SetUIStatus(true);
            }
            else
            {
                SetUIStatus(false);
            }
        }

        private void SetUIStatus(bool enabled)
        {
            if (enabled)
            {
                lblHint.Text = "";
            }
            else
            {
                lblHint.Text = "提示：注册表中不存在玩家名项，请先运行一次游戏客户端！";
            }

            lblPlayerName.Enabled = enabled;
            txtPlayerName.Enabled = enabled;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string NewPlayerName = txtPlayerName.Text;
            RegistryHelper.WriteStringToRegistryBinary(RegistryPath, RegistryKeyName, NewPlayerName);
            CurrentValue = RegistryHelper.ConvertRegistryBinaryToString(RegistryPath, RegistryKeyName);
            if (CurrentValue.Equals(NewPlayerName))
            {
                MessageBoxEx.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnOK.Enabled = false;
            }
            else
            {
                MessageBoxEx.Show("保存失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtPlayerName_TextChanged(object sender, EventArgs e)
        {
            bool NeedSave = false;

            string NewValue = txtPlayerName.Text;
            if (!string.IsNullOrEmpty(NewValue))
            {
                if (!NewValue.Equals(CurrentValue))
                {
                    NeedSave = true;
                }
            }

            btnOK.Enabled = NeedSave;
        }

    }
}
