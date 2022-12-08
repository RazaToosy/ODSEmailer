using System;
using System.Diagnostics;
using System.Net;
using Microsoft.Exchange.WebServices.Data;

namespace Emailer
{
    /// <summary>
    /// Sends Email via Exchange Server
    /// </summary>
    public class SendEmail
    {
        private ModelLoginDetails _loginDetails;
        private string _responseMessage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="LoginDetails"></param>
        public SendEmail(ModelLoginDetails LoginDetails)
        {
            _loginDetails = LoginDetails;

        }

        /// <summary>
        /// Accessor for Response Message
        /// </summary>
        public string ResponseMessage
        {
            get { return _responseMessage; }
            set { _responseMessage = value; }
        }


        public bool DetailsWithAttachment(ModelEmailDetails emailDetails)
        {
            bool isSuccessful = true;
            try
            {
                var service = new ExchangeService(ExchangeVersion.Exchange2010_SP1);
                NetworkCredential cred = new NetworkCredential(_loginDetails.UserName, _loginDetails.Password, _loginDetails.Domain);
                Uri url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
                service.Url= url;
                service.Credentials = cred;

                //Create an email message and initialize it with the from address, to address, subject and the body of the email.
                // Create an email message and set properties on the message.
                EmailMessage message = new EmailMessage(service);
                // Set properties on the email message.
                message.Subject = emailDetails.SubjectOfEmail;
                message.Body = emailDetails.BodyOfEmail;
                message.ToRecipients.Add(emailDetails.RecepientEmail);
                // Add a file attachment by using the fully qualified location of the file. 
                message.Attachments.AddFileAttachment(emailDetails.AttachmentLocation);

                // Add a file attachment by using the fully qualified string name, 
                // and specify the name of the attachment as it will appear in the email.
                // The new name of the file attachment is SecondAttachment.txt.
                //message.Attachments.AddFileAttachment("SecondAttachment.txt", "C:\\temp\\FileAttachment2.txt");

                // Add a file attachment by using a byte array.
                // In this example, theBytes is the byte array that represents the content of the image file to attach.
                //byte[] theBytes = File.ReadAllBytes("C:\\Temp\\Tulips.jpg");

                // The byte array file attachment is named ThirdAttachment.jpg.
                // message.Attachments.AddFileAttachment("ThirdAttachment.jpg", theBytes);
                // Add a file attachment by using a stream.
                //FileStream theStream = new FileStream("C:\\temp\\FileAttachment4.txt", FileMode.OpenOrCreate);

                // The streamed file attachment is named FourthAttachment.txt.
                //message.Attachments.AddFileAttachment("FourthAttachment.txt", theStream);

                // Add an email message as an item attachment and set properties on the item.
                //ItemAttachment<EmailMessage> itemAttachment = message.Attachments.AddItemAttachment<EmailMessage>();
                //itemAttachment.Name = emailDetails.SubjectOfEmail;
                //itemAttachment.Item.Subject = emailDetails.SubjectOfEmail;
                //itemAttachment.Item.Body = emailDetails.BodyOfEmail;
                //itemAttachment.Item.ToRecipients.Add(emailDetails.RecepientEmail);

                // Send the mail and save a copy in the Sent Items folder.
                // This method results in a CreateItem and SendItem call to EWS.
                message.SendAndSaveCopy();
            }
            catch (Exception err)
            {
                isSuccessful = false;
                string errorMessage = string.Format("Error in sending Emails to Server {0} - {1}", err.Message,
                                                   err.StackTrace);

                _responseMessage = errorMessage;
                Logger.LogWriter.Instance.WriteToLog(errorMessage);
                Debug.WriteLine(errorMessage);
                return isSuccessful;
            }

            return isSuccessful;
        }
    }
}
