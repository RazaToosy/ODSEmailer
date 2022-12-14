using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommissioningMailer;
using Emailer.UI.Properties;
using ProxyHelpers.EWS;
//using Microsoft.Exchange.WebServices;

namespace Emailer.UI.Controller
{
    class ExportEmails
    {
        private MainEmailerForm _form;
        private BackgroundWorker _oWorker;
        private IEnumerable<MailInfo> _mailerInfo;

        public ExportEmails(MainEmailerForm theForm)
        {
            _form = theForm;
            OWorker = new BackgroundWorker();
            OWorker.DoWork += oWorker_DoWork;
            OWorker.ProgressChanged += oWorker_ProgressChanged;
            OWorker.RunWorkerCompleted += oWorker_RunWorkerCompleted;
            OWorker.WorkerReportsProgress = true;
            OWorker.WorkerSupportsCancellation = true;
 
        }

        public BackgroundWorker OWorker
        {
            get { return _oWorker; }
            set { _oWorker = value; }
        }


        public void oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _oWorker.ReportProgress(0,string.Empty);

            Login login = new Login("https://outlook.office365.com/EWS/Exchange.asmx)");
            login.ShowDialog();

            BodyType type = new BodyType();
            type.BodyType1 = (BodyTypeType)Enum.Parse(typeof(BodyTypeType), Settings.Default.BodyType);

            int currentrow = 0;

            foreach (MailInfo mailInfo in _mailerInfo)
            {
                login.Send.DetailsWithAttachment(new ModelEmailDetails
                {
                    SubjectOfEmail = string.Format("{0} for {1}", _form.TextBoxSubject, mailInfo.Key),
                    BodyOfEmail = _form.TextBoxBody,
                    SenderEmail = _form.TextBoxSender,
                    RecepientEmail = mailInfo.EmailAddress,
                    //AttachmentLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    //                            @"GitHub\NhsCommissioningMailer\CommissioningMailer\SampleData\") + "KeyEmailAddressPair.csv",
                    AttachmentLocation = mailInfo.AttachmentPath,
                    BodyType = type,
                    ContentType = Settings.Default.ContentType
                });
                currentrow++;
                double percentage = ((double)currentrow / (double)_mailerInfo.Count())*100;
                _oWorker.ReportProgress(Convert.ToInt32(percentage), string.Format("{0} of {1} emails sent", currentrow, _mailerInfo.Count()));

                if (_oWorker.CancellationPending)
                {
                    e.Cancel = true;
                    _oWorker.ReportProgress(0,string.Empty);
                    return;
                }
            }

            OWorker.ReportProgress(100);
        }


        public void BtnCancelClick(object sender, EventArgs e)
        {
            if (OWorker.IsBusy)
            {
                //Stop/Cancel the async operation here
                OWorker.CancelAsync();
            }
        }


        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //If it was cancelled midway
            if (e.Cancelled)
            {
                MessageBox.Show("Operation Cancelled");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error while performing background operation.");
            }
            else
            {
                MessageBox.Show("Emails Completed...");
            }
            
            _form.ButtonSendEmail.Text = "Send Emails";
            removeTempFiles();

        }

        private void removeTempFiles()
        {
            try
            {
                string[] picList = Directory.GetFiles(Path.GetTempPath(), "*.csv");
                foreach (string file in picList)
                {
                    File.Delete(file);
                }
            }
            catch (IOException ex)
            {
                Logger.LogWriter.Instance.WriteToLog(string.Format("Clean Up Delete File Message--{0}\n\n", ex.Message));
            }
        }


        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Here you play with the main UI thread
            _form.ProgressBarForEmails.Value = e.ProgressPercentage;
            if (e.UserState != null) _form.LabelNoOfEmailsSent.Text = e.UserState.ToString();
        }

        public void SplitCsv(string keyedDataFilePath)
        {
            var surgeries = new KeyEmailAddressPairRepository(Settings.Default.KeyEmailFilePath).GetAll();
            var data = new KeyedDataRepository(keyedDataFilePath).GetAll();
            _mailerInfo = CsvWriter.WriteCsvFiles(surgeries, data);
        }
    }
}
