﻿using B2CDevSync.Models;
using B2CDevSync.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Forms;

namespace B2CDevSync
{
    public partial class InitTenant : Form
    {
        private Main _parent;
        private IList<App> appList;
        private IList<App> b2c;
        private IList<App> aad;
        private IList<PolicyItem> policies;
        private App testApp;
        private App iefApp;
        private App iefProxyApp;
        private IList<TFKeyset> keys;
        private TFKeyset signingKey;
        private TFKeyset encryptionKey;
        private TFKeyset facebookKey;
        private string baseTenant;

        public InitTenant(Main parent)
        {
            InitializeComponent();
            _parent = parent;
            baseTenant = _parent._settings.B2CTenant.Split('.')[0];
            txtFacebookAppID.Text = _parent._settings.FacebookAppId;
        }

        private void CheckRepoStatus()
        {
            if (File.Exists(_parent._policy.ZipPath))
            {
                var file = File.GetLastWriteTime(_parent._policy.ZipPath);
                txtRepoStatus.Text = string.Format("Downloaded {0}", file.ToString("MM/dd/yyyy hh:mmtt "));
            }
            else
            {
                txtRepoStatus.Text = "Not yet downloaded";
            }
        }
        private void ClearStatus()
        {
            btnSetupPolicies.Enabled = false;
            lstPolicies.Items.Clear();
            txtTestApplication.Text = "";
            lstTenantApps.Items.Clear();
            lstB2CApps.DataSource = null;
            lstB2CApps.Items.Clear();
            txtIEFApp.Text = "";
            txtIEFProxyApp.Text = "";
            txtSigningKey.Text = "";
            txtEncryptionKey.Text = "";
            txtFacebookKey.Text = "";

        }
        private void CheckReadiness()
        {
            var isReady = true;

            btnCreateTestApp.Enabled = false;
            btnCreateIEFApp.Enabled = false;
            btnCreateIEFProxyApp.Enabled = false;
            btnCreateSigningKey.Enabled = false;
            btnCreateEncKey.Enabled = false;
            btnCreateFBKey.Enabled = false;
            consentTestApp.Visible = false;
            consentIEF.Visible = false;
            consentIEFProxy.Visible = false;

            if (testApp == null)
            {
                isReady = false;
                btnCreateTestApp.Enabled = true;
            } 
            else
            {
                consentTestApp.Visible = true;
                consentTestApp.Links.Clear();
                consentTestApp.Links.Add(0, 10, GetConsentUrl(testApp.AppId));
            }

            if (iefApp == null)
            {
                isReady = false;
                btnCreateIEFApp.Enabled = true;
            }
            else
            {
                consentIEF.Visible = true;
                consentIEF.Links.Clear();
                consentIEF.Links.Add(0, 10, GetConsentUrl(iefApp.AppId));
            }

            if (iefProxyApp == null)
            {
                isReady = false;
                btnCreateIEFProxyApp.Enabled = true;
            }
            else
            {
                consentIEFProxy.Visible = true;
                consentIEFProxy.Links.Clear();
                consentIEFProxy.Links.Add(0, 10, GetConsentUrl(iefProxyApp.AppId));
            }

            if (signingKey == null)
            {
                isReady = false;
                btnCreateSigningKey.Enabled = true;
            }
            if (encryptionKey == null)
            {
                isReady = false;
                btnCreateEncKey.Enabled = true;
            }
            if (facebookKey == null)
            {
                isReady = false;
                btnCreateFBKey.Enabled = true;
            }
            if (txtFacebookAppID.Text.Length == 0)
            {
                isReady = false;
            }
            if (txtRepoStatus.Text == "" || txtRepoStatus.Text == "Not yet downloaded")
            {
                isReady = false;
            }

            if (isReady)
            {
                btnSetupPolicies.Enabled = true;
                txtNewPolicyName.Text = TestPolicyName(txtNewPolicyName.Text);
            }
        }

        private string TestPolicyName(string proposedName)
        {
            var pattern = string.Format(@"^*{0}[\d{{2}}]*Base", proposedName);
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            if (lstPolicies.Items.Count > 0)
            {
                var lst = lstPolicies.Items.OfType<string>().Where(i => rgx.IsMatch(i.ToString())).ToList();
                if (lst.Count > 0)
                {
                    proposedName = string.Format("{0}{1}", proposedName, (lst.Count + 1).ToString());
                }
            }
            return proposedName;
        }
        private async Task GetStatus()
        {
            aniRunning.Visible = true;
            try
            {
                ClearStatus();

                //get policies
                policies = await _parent._policy.GetListAsync();
                if (policies != null && policies.Count > 0)
                {
                    foreach (var item in policies)
                    {
                        lstPolicies.Items.Add(item.Id);
                    }
                }
                else
                {
                    lstPolicies.Items.Add("N/A");
                }

                //get applications
                appList = await _parent._apps.GetAppListAsync();
                if (appList != null)
                {
                    b2c = appList.Where(a => a.SignInAudience == Models.Audiences.AzureADandPersonalMicrosoftAccount).ToList();
                    if (b2c !=null && b2c.Count > 0)
                    {
                        lstB2CApps.DisplayMember = "DisplayName";
                        lstB2CApps.ValueMember = "Id";
                        lstB2CApps.DataSource = b2c;
                    }
                    else
                    {
                        lstB2CApps.Items.Add("N/A");
                    }

                    //check for test app
                    testApp = b2c.FirstOrDefault(a => a.Web.RedirectUris.Any(b => b == "https://jwt.ms"));
                    if (testApp != null)
                    {
                        txtTestApplication.Text = GetAppName(testApp);
                        btnCreateTestApp.Enabled = false;
                    }
                    else
                    {
                        txtTestApplication.Text = "N/A";
                    }

                    aad = appList.Where(a => a.SignInAudience != Models.Audiences.AzureADandPersonalMicrosoftAccount).ToList();
                    if (aad != null && aad.Count>0)
                    {
                        foreach (var item in aad)
                        {
                            lstTenantApps.Items.Add(item.DisplayName);
                        }
                    }
                    else
                    {
                        lstTenantApps.Items.Add("N/A");
                    }

                    //check for IdentityExperienceFramework app
                    iefApp = aad.FirstOrDefault(a => a.DisplayName == "IdentityExperienceFramework");
                    if (iefApp != null)
                    {
                        txtIEFApp.Text = string.Format("{0} ({1})", iefApp.DisplayName, iefApp.Id);
                        btnCreateIEFApp.Enabled = false;
                    }
                    else
                    {
                        txtIEFApp.Text = "N/A";
                    }

                    //check for ProxyIdentityExperienceFramework app
                    iefProxyApp = aad.FirstOrDefault(a => a.DisplayName == "ProxyIdentityExperienceFramework");
                    if (iefProxyApp != null)
                    {
                        txtIEFProxyApp.Text = string.Format("{0} ({1})", iefProxyApp.DisplayName, iefProxyApp.Id);
                        btnCreateIEFProxyApp.Enabled = false;
                    }
                    else
                    {
                        txtIEFProxyApp.Text = "N/A";
                    }
                }
                else
                {
                    lstB2CApps.DataSource = null;
                    lstB2CApps.Items.Add("N/A");
                    txtTestApplication.Text = "N/A";
                    lstTenantApps.Items.Add("N/A");
                    txtIEFApp.Text = "N/A";
                    txtIEFProxyApp.Text = "N/A";
                }

                //check for keys
                keys = await _parent._policy.GetKeySetsAsync();
                if (keys != null)
                {
                    signingKey = keys.SingleOrDefault(k => k.Id.Contains("TokenSigningKeyContainer"));
                    if (signingKey != null)
                    {
                        txtSigningKey.Text = signingKey.Id;
                        btnCreateSigningKey.Enabled = false;
                    }
                    else
                    {
                        txtSigningKey.Text = "N/A";
                    }
                    encryptionKey = keys.SingleOrDefault(k => k.Id.Contains("TokenEncryptionKeyContainer"));
                    if (encryptionKey != null)
                    {
                        txtEncryptionKey.Text = encryptionKey.Id;
                        btnCreateEncKey.Enabled = false;
                    }
                    else
                    {
                        txtEncryptionKey.Text = "N/A";
                    }
                    facebookKey = keys.SingleOrDefault(k => k.Id.Contains("FacebookSecret"));
                    if (facebookKey != null)
                    {
                        txtFacebookKey.Text = facebookKey.Id;
                        btnCreateFBKey.Enabled = false;
                    }
                    else
                    {
                        txtFacebookKey.Text = "N/A";
                    }
                }
                else
                {
                    txtSigningKey.Text = "N/A";
                    txtEncryptionKey.Text = "N/A";
                    txtFacebookKey.Text = "N/A";
                }

                //get repo status
                CheckRepoStatus();

                aniRunning.Visible = false;
                CheckReadiness();
            }
            catch (Exception ex)
            {
                aniRunning.Visible = false;
                MessageBox.Show(ex.Message, "Error retrieving some tenant IEF properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetStatus_Click(object sender, EventArgs e)
        {
            await GetStatus();
        }

        private string GetAppName(App app)
        {
            return string.Format("{0} ({1})", app.DisplayName, app.Id);
        }
        private async void btnCreateTestApp_Click(object sender, EventArgs e)
        {
            aniRunning.Visible = true;

            var app = new App
            {
                DisplayName = "Test App",
                IsFallbackPublicClient = false,
                SignInAudience = Audiences.AzureADandPersonalMicrosoftAccount
            };
            app.Web.RedirectUris.Add("https://jwt.ms");
            app.Web.ImplicitGrantSettings.EnableAccessTokenIssuance = true;
            app.Web.ImplicitGrantSettings.EnableIdTokenIssuance = true;

            var accessList = new List<ResourceAccess>
            {
                new ResourceAccess
                {
                    Id = "37f7f235-527c-4136-accd-4a02d197296e",
                    Type = "Scope"
                },
                new ResourceAccess
                {
                    Id = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182",
                    Type = "Scope"
                }
            };

            app.RequiredResourceAccess.Add(new RequiredResourceAccess
            {
                ResourceAppId = "00000003-0000-0000-c000-000000000000",
                ResourceAccess = accessList
            });

            testApp = await _parent._apps.CreateB2CApp(app);
            if (testApp == null)
            {
                toolStripStatusLabel1.Text = _parent._policy.LastError;
                return;
            }
            txtTestApplication.Text = GetAppName(testApp);
            aniRunning.Visible = false;

            CheckReadiness();
        }

        private async void btnCreateIEFApp_Click(object sender, EventArgs e)
        {
            aniRunning.Visible = true;

            var app = new App
            {
                DisplayName = "IdentityExperienceFramework",
                SignInAudience = Audiences.AzureADMyOrg,
                
            };
            app.Web.RedirectUris.Add(string.Format("https://{0}.b2clogin.com/{0}.onmicrosoft.com", baseTenant));
            app.Web.HomePageUrl = string.Format("https://login.microsoftonline.com/{0}.onmicrosoft.com", baseTenant);
            app.Web.ImplicitGrantSettings.EnableIdTokenIssuance = true;
            app.Web.ImplicitGrantSettings.EnableAccessTokenIssuance = false;
            app.IdentifierUris.Add(string.Format("https://{0}.onmicrosoft.com/{1}", baseTenant, Guid.NewGuid().ToString()));

            var accessList = new List<ResourceAccess>
            {
                new ResourceAccess
                {
                    Id = "37f7f235-527c-4136-accd-4a02d197296e",
                    Type = "Scope"
                },
                new ResourceAccess
                {
                    Id = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182",
                    Type = "Scope"
                }
            };

            app.RequiredResourceAccess.Add(new RequiredResourceAccess
            {
                ResourceAppId = "00000003-0000-0000-c000-000000000000",
                ResourceAccess = accessList
            });

            iefApp = await _parent._apps.CreateApp(app);
            if (iefApp == null)
            {
                toolStripStatusLabel1.Text = _parent._apps.LastError;
                aniRunning.Visible = false;
                return;
            }

            //give the app a sec to settle in
            Thread.Sleep(2000);

            //add API scopes
            var perm = new OAuth2Permission
            {
                AdminConsentDescription = "Allow the application to access IdentityExperienceFramework on behalf of the signed-in user.",
                AdminConsentDisplayName = "Access IdentityExperienceFramework",
                Id = Guid.NewGuid().ToString(),
                IsEnabled = true,
                //Origin = "Application",
                Type = "Admin",
                Value = "user_impersonation",
                UserConsentDescription = null,
                UserConsentDisplayName = null
            };
            var apiApp = new ApiApp(1);
            apiApp.Oauth2PermissionScopes.Add(perm);

            iefApp.Api = apiApp;

            var success = await _parent._apps.PublishApiScopes(iefApp, apiApp);
            if (!success)
            {
                toolStripStatusLabel1.Text = _parent._apps.LastError;
                aniRunning.Visible = false;
                return;
            }
            txtIEFApp.Text = GetAppName(iefApp);
            aniRunning.Visible = false;

            CheckReadiness();
        }

        private async void btnCreateIEFProxyApp_Click(object sender, EventArgs e)
        {
            if (iefApp == null)
            {
                toolStripStatusLabel1.Text = "The IdentityExperienceFramework app must be created first";
                return;
            }
            aniRunning.Visible = true;

            var app = new App
            {
                DisplayName = "ProxyIdentityExperienceFramework",
                SignInAudience = Audiences.AzureADMyOrg,
             };
            app.PublicClient.RedirectUris.Add("myapp://auth");
            app.IsFallbackPublicClient = true;
            app.Web.ImplicitGrantSettings.EnableIdTokenIssuance = false;
            app.Web.ImplicitGrantSettings.EnableAccessTokenIssuance = false;

            var acc = new RequiredResourceAccess
            {
                ResourceAppId = "00000003-0000-0000-c000-000000000000",
            };
            acc.ResourceAccess.Add(new ResourceAccess
            {
                Id = "37f7f235-527c-4136-accd-4a02d197296e",
                Type = "Scope"
            });
            acc.ResourceAccess.Add(new ResourceAccess
            {
                Id = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182",
                Type = "Scope"
            });
            app.RequiredResourceAccess.Add(acc);

            acc = new RequiredResourceAccess
            {
                ResourceAppId = iefApp.AppId,
            };
            acc.ResourceAccess.Add(new ResourceAccess
            {
                Id = iefApp.Api.Oauth2PermissionScopes.First().Id,
                Type = "Scope"
            });
            app.RequiredResourceAccess.Add(acc);
            iefProxyApp = await _parent._apps.CreateApp(app);
            if (iefProxyApp == null)
            {
                toolStripStatusLabel1.Text = _parent._policy.LastError;
                return;
            }
            txtIEFProxyApp.Text = GetAppName(iefProxyApp);
            aniRunning.Visible = false;

            CheckReadiness();
        }

        private string GetConsentUrl(string appId)
        {
            var tenantId = _parent._settings.B2CTenant;
            var url = string.Format("https://login.microsoftonline.com/{0}/adminconsent?client_id={1}", tenantId, appId);
            //var nonce = Guid.NewGuid().ToString();
            //var url = string.Format("https://login.microsoftonline.com/{0}/oauth2/authorize?response_type=id_token&client_id={1}&redirect_uri=http://localhost&response_mode=form_post&nonce={2}&resource=https://graph.microsoft.com&prompt=admin_consent", tenantId, appId, nonce);
            return url;
        }

        private async void btnCreateSigningKey_Click(object sender, EventArgs e)
        {
            aniRunning.Visible = true;

            signingKey = await _parent._policy.CreateKeyset("TokenSigningKeyContainer");
            if (signingKey == null)
            {
                toolStripStatusLabel1.Text = _parent._policy.LastError;
                return;
            }
            var key = await _parent._policy.GenerateKey(signingKey, TFKeyKty.rsa, TFKeyUse.sig);
            if (key == null)
            {
                //keygen failed - delete the keyset, show the error, and let the user try again
                await _parent._policy.DeleteKeyset(signingKey);
                signingKey = null;
                var msg = string.Format("An error occured creating the key. The keyset has been deleted so you can try again after correcting the issue.\n\rError:\n\r{0}", _parent._policy.LastError);
                MessageBox.Show(msg, "Error Creating Keyset");
                return;
            }
            txtSigningKey.Text = signingKey.Id;
            aniRunning.Visible = false;

            CheckReadiness();
        }

        private async void btnCreateEncKey_Click(object sender, EventArgs e)
        {
            aniRunning.Visible = true;

            encryptionKey = await _parent._policy.CreateKeyset("TokenEncryptionKeyContainer");
            if (encryptionKey == null)
            {
                toolStripStatusLabel1.Text = _parent._policy.LastError;
                return;
            }
            var key = await _parent._policy.GenerateKey(encryptionKey, TFKeyKty.rsa, TFKeyUse.enc);
            if (key == null)
            {
                //keygen failed - delete the keyset, show the error, and let the user try again
                await _parent._policy.DeleteKeyset(encryptionKey);
                encryptionKey = null;
                var msg = string.Format("An error occured creating the key. The keyset has been deleted so you can try again after correcting the issue.\n\rError:\n\r{0}", _parent._policy.LastError);
                MessageBox.Show(msg, "Error Creating Keyset");
                return;
            }

            txtEncryptionKey.Text = encryptionKey.Id;
            aniRunning.Visible = false;

            CheckReadiness();
        }

        private async void btnCreateFBKey_Click(object sender, EventArgs e)
        {
            aniRunning.Visible = true;

            var fbSecret = Dialog.ShowDialog("Please enter the secret from your Facebook app registration:", "Facebook Secret");
            if (fbSecret == "")
                return;

            facebookKey = await _parent._policy.CreateKeyset("FacebookSecret");
            if (facebookKey == null)
            {
                toolStripStatusLabel1.Text = _parent._policy.LastError;
                return;
            }
            var key = await _parent._policy.UploadKeysetSecret(facebookKey, fbSecret, TFKeyUse.sig);
            if (key == null)
            {
                //keygen failed - delete the keyset, show the error, and let the user try again
                await _parent._policy.DeleteKeyset(facebookKey);
                facebookKey = null;
                var msg = string.Format("An error occured creating the key. The keyset has been deleted so you can try again after correcting the issue.\n\rError:\n\r{0}", _parent._policy.LastError);
                MessageBox.Show(msg, "Error Creating Keyset");
                return;
            }

            txtFacebookKey.Text = facebookKey.Id;
            aniRunning.Visible = false;

            CheckReadiness();
        }

        private async void btnSetupPolicies_Click(object sender, EventArgs e)
        {
            aniRunning.Visible = true;

            var dlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Select a place to deposit your modified files before they are uploaded",
                RootFolder = Environment.SpecialFolder.MyComputer
            };

            var dlgRes = dlg.ShowDialog();
            if (dlgRes == DialogResult.OK)
            {
                var newFolderPath = dlg.SelectedPath;
                var selectedPolicy = groupBox1.Controls.OfType<RadioButton>().Single(n => n.Checked);

                await _parent._policy.ReadAndReplacePolicyTemplate(newFolderPath, selectedPolicy.Text.Replace(" ", ""), baseTenant, iefApp.AppId, iefProxyApp.AppId, txtNewPolicyName.Text, txtFacebookAppID.Text);
            }
            aniRunning.Visible = false;
            ShowStatus("Policies created successfully");

            await GetStatus();
        }

        private void ShowStatus(string message)
        {
            statusStrip1.Text = message;
            var task = Task.Run(() =>
            {
                Thread.Sleep(3000);
                Invoke(new Action(() =>
                {
                    statusStrip1.Text = "";
                }));
            });
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private async void InitTenant_Load(object sender, EventArgs e)
        {
            _parent._policy.RefreshToken(_parent._auth.Token);
            _parent._apps.RefreshToken(_parent._auth.Token);
            txtTenantName.Text = _parent._settings.B2CTenant;
            await GetStatus();
            CheckRepoStatus();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitLink();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link that was clicked.");
            }
        }
        private void VisitLink()
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabel1.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var folder = Directory.GetParent(_parent._policy.ZipPath);
            System.Diagnostics.Process.Start(folder.FullName);
        }

        private async void btnPullRepo_Click_1(object sender, EventArgs e)
        {
            btnPullRepo.Enabled = false;
            var fileProgIndicator = new Progress<int>(ReportDlProgress);
            lblDownloading.Visible = true;
            prgDownload.Visible = true;
            await _parent._policy.RefreshStarterpack(fileProgIndicator);
            lblDownloading.Visible = false;
            prgDownload.Visible = false;
            CheckRepoStatus();
            CheckReadiness();

            btnPullRepo.Enabled = true;
        }

        void ReportDlProgress(int value)
        {
            if (value == -1)
            {
                StartNoProgDownload();
                return;
            }
            if (value == 100 && timer.Enabled)
            {
                timer.Stop();
            }
            prgDownload.Value = value;
        }

        System.Windows.Forms.Timer timer;
        private void StartNoProgDownload()
        {
            timer = new System.Windows.Forms.Timer
            {
                Interval = 80
            };
            timer.Tick += (sender, e) =>
            {
                if (prgDownload.Value == 100)
                {
                    prgDownload.Value = 1;
                }
                else
                {
                    prgDownload.Value++;
                }
            };
            timer.Start();
        }

        private void rdoLocal_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtFacebookAppID_Leave(object sender, EventArgs e)
        {
            _parent._settings.FacebookAppId = txtFacebookAppID.Text;
            _parent._settings.Save();
            CheckReadiness();
        }

        private void consentTestApp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(consentTestApp.Links[0].LinkData.ToString());
        }

        private void consentIEF_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(consentIEF.Links[0].LinkData.ToString());
        }

        private void consentIEFProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(consentIEFProxy.Links[0].LinkData.ToString());
        }
    }
}
