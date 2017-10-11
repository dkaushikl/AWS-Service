using System;
using System.Collections.Generic;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Amazon.S3;
using Amazon.S3.Model;
using System.Web;
using MimeKit;

namespace DemoAmazon
{
    public class Ses
    {
        public string SenderAddress { get; set; }
        public string ReceiverAddress { get; set; }

        private const string AwsAccessKeyId = "Access Key ID";
        private const string AwsSecretAccessKey = "Access Secret Key ID";

        // provide ses.
        public string SmtpUsername { get; set; } = "SMTP Username";
        public string SmtpPassword { get; set; } = "SMTP Password";

        public string Host { get; set; } = "email-smtp.us-west-2.amazonaws.com";
        public int Port { get; set; } = 465;
        public RegionEndpoint EndPoint { get; set; } = RegionEndpoint.USWest2;
        public static void Log_text_File(string content)
        {
            var appFullPath = HttpContext.Current.Server.MapPath(@"\Error");
            if (!Directory.Exists(appFullPath))
            {
                Directory.CreateDirectory(appFullPath);
            }
            var fs = new FileStream(appFullPath + @"\ErrorLog.txt", FileMode.OpenOrCreate, FileAccess.Write);

            var sw = new StreamWriter(fs);

            sw.BaseStream.Seek(0, SeekOrigin.End);

            sw.WriteLine(content);

            sw.Flush();
            sw.Close();
        }
        public void Main(string sender, string receiver)
        {
            SenderAddress = SenderAddress;
            ReceiverAddress = ReceiverAddress;
            if (CheckRequiredFields())
            {
                using (var client = new AmazonSimpleEmailServiceClient(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
                {
                    CloneReceiptRule(client);
                    UpdateReceiptRule(client, SenderAddress);
                    CreateEmail(client, SenderAddress);
                    CreateReceiptRuleSet(client, SenderAddress);
                    CreateBucket(SenderAddress.Replace("@", "-").Replace(".", "-"));
                    CreateReceiptRule(client, SenderAddress);
                    GetAllVerifiedEmail(client);
                    //VerifyEmailAddress(client, SenderAddress);
                    SendEmail(client);
                    SendEmailWithAttachments(client);
                    SendSmtpEmail();
                }
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        public void GetAllVerifiedEmail(IAmazonSimpleEmailService client)
        {
            var lstVerifiedEmailAddresses = client.ListVerifiedEmailAddresses();
            foreach (var objEmail in lstVerifiedEmailAddresses.VerifiedEmailAddresses)
            {
                Console.Write(objEmail + "\n");
            }
        }

        public void CreateEmail(IAmazonSimpleEmailService client, string email)
        {
            if (CheckEmailExist(client, email)) return;

            var objVerifyEmailAddress = new VerifyEmailAddressRequest { EmailAddress = email };
            client.VerifyEmailAddress(objVerifyEmailAddress);
        }

        public void DeleteEmail(IAmazonSimpleEmailService client, string email)
        {
            if (CheckEmailExist(client, email)) return;
            var objDeleteVerifiedEmailAddressRequest = new DeleteVerifiedEmailAddressRequest { EmailAddress = email };
            client.DeleteVerifiedEmailAddress(objDeleteVerifiedEmailAddressRequest);
        }

        public bool CheckEmailExist(IAmazonSimpleEmailService client, string email)
        {
            var lstVerifiedEmailAddresses = client.ListVerifiedEmailAddresses();
            return lstVerifiedEmailAddresses.VerifiedEmailAddresses.Any(s => s.Contains(email));
        }

        public void CreateReceiptRuleSet(IAmazonSimpleEmailService client, string email)
        {
            try
            {
                var objCreateReceiptRuleSetRequest = new CreateReceiptRuleSetRequest
                {
                    RuleSetName = email.Replace("@", "-")
                };
                client.CreateReceiptRuleSet(objCreateReceiptRuleSetRequest);
            }
            catch
            {
                // ignored
            }
        }
        public void CreateReceiptRule(IAmazonSimpleEmailService client, string email)
        {
            try
            {
                var objCreateReceiptRuleRequest = new CreateReceiptRuleRequest
                {
                    RuleSetName = email.Replace("@", "-")
                };

                var lstReceiptAction = new List<ReceiptAction>();
                var objReceiptAction = new ReceiptAction();
                var objS3Action = new S3Action
                {
                    BucketName = email.ToLower().Replace("@", "-").Replace(".", "-"),
                    ObjectKeyPrefix = string.Empty,
                    TopicArn = "arn:aws:sns:us-west-2:803078631911:test-email",
                    KmsKeyArn = string.Empty,
                };
                objReceiptAction.S3Action = objS3Action;

                lstReceiptAction.Add(objReceiptAction);

                var objReceiptRule = new ReceiptRule
                {
                    Name = email.Substring(0, 3),
                    ScanEnabled = true,
                    Enabled = true,
                    Actions = lstReceiptAction,
                    TlsPolicy = TlsPolicy.Optional,
                    Recipients = new List<string> { email }

                };
                objCreateReceiptRuleRequest.Rule = objReceiptRule;
                client.CreateReceiptRule(objCreateReceiptRuleRequest);
            }
            catch
            {
                // ignored
            }
        }

        public void UpdateReceiptRule(IAmazonSimpleEmailService client, string email)
        {
            try
            {
                var objReceipient = new List<string>(DescribeReceiptRule(client));
                objReceipient.Add(SenderAddress);
                var objCreateReceiptRuleRequest = new UpdateReceiptRuleRequest
                {
                    RuleSetName = "email-receive"
                };

                var lstReceiptAction = new List<ReceiptAction>();
                var objReceiptAction = new ReceiptAction();
                var objS3Action = new S3Action
                {
                    BucketName = "test-receive-email",
                    ObjectKeyPrefix = string.Empty,
                    TopicArn = "arn:aws:sns:us-west-2:803078631911:test-notification",
                    KmsKeyArn = string.Empty
                };
                objReceiptAction.S3Action = objS3Action;

                lstReceiptAction.Add(objReceiptAction);

                var objReceiptRule = new ReceiptRule
                {
                    Name = "test-email",
                    ScanEnabled = true,
                    Enabled = true,
                    Actions = lstReceiptAction,
                    TlsPolicy = TlsPolicy.Optional,
                    Recipients = objReceipient
                };
                objCreateReceiptRuleRequest.Rule = objReceiptRule;
                client.UpdateReceiptRule(objCreateReceiptRuleRequest);
            }
            catch
            {
                // ignored
            }
        }

        public List<string> DescribeReceiptRule(IAmazonSimpleEmailService client)
        {
            try
            {
                var objDescribeReceiptRuleRequest = new DescribeReceiptRuleSetRequest
                {
                    RuleSetName = "email-receive"
                };
                var response = client.DescribeReceiptRuleSet(objDescribeReceiptRuleRequest);
                return response.Rules[0].Recipients;
            }
            catch
            {
                // ignored
            }
            return null;
        }

        public void CloneReceiptRule(IAmazonSimpleEmailService client)
        {
            try
            {
                var objCloneReceiptRuleSetRequest = new CloneReceiptRuleSetRequest
                {
                    RuleSetName = "test-rule",
                    OriginalRuleSetName = "test"
                };
                client.CloneReceiptRuleSet(objCloneReceiptRuleSetRequest);
            }
            catch
            {
                // ignored
            }
        }

        public bool CheckBucketExist(AmazonS3Client client, string bucketName)
        {
            var lstBuckets = client.ListBuckets();
            return lstBuckets.Buckets.Any(s => s.BucketName == bucketName);
        }

        public void CreateBucket(string bucketName)
        {
            using (var client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                if (CheckBucketExist(client, bucketName)) return;

                var request = new PutBucketRequest
                {
                    BucketName = bucketName,
                    BucketRegion = S3Region.USW2,
                    CannedACL = S3CannedACL.PublicReadWrite
                };
                client.PutBucket(request);
            }
        }

        //private static void VerifyEmailAddress(IAmazonSimpleEmailService client, string email)
        //{
        //    var objVerifyEmailIdentityRequest = new VerifyEmailIdentityRequest { EmailAddress = email };
        //    client.VerifyEmailIdentity(objVerifyEmailIdentityRequest);
        //}

        private bool CheckRequiredFields()
        {
            if (string.IsNullOrEmpty(SenderAddress))
            {
                Console.WriteLine("The variable senderAddress is not set.");
                return false;
            }
            if (!string.IsNullOrEmpty(ReceiverAddress)) return true;
            Console.WriteLine("The variable receiverAddress is not set.");
            return false;
        }



        private void SendEmail(IAmazonSimpleEmailService client)
        {
            var lstReplyToAddress = new List<string>
                    {
                        "dhameliyakaushik13@gmail.com"
                    };
            var sendRequest = new SendEmailRequest
            {
                Source = SenderAddress,
                Destination = new Destination { ToAddresses = new List<string> { ReceiverAddress } },
                ReplyToAddresses = lstReplyToAddress,
                Message = new Message
                {
                    Subject = new Content("Sample Mail using SES"),
                    Body = new Body { Text = new Content("Sample message content.") }
                }
            };
            var response = client.SendEmail(sendRequest);
            var messageId = response.MessageId;
            Console.Write("Messageid: {0}\n", messageId);
        }




        private void SendEmailWithAttachments(IAmazonSimpleEmailService client)
        {
            using (var messageStream = new MemoryStream())
            {
                var message = new MimeMessage();
                var builder = new BodyBuilder { TextBody = "Hello World" };

                message.From.Add(new MailboxAddress(SenderAddress));
                message.To.Add(new MailboxAddress(ReceiverAddress));
                message.Subject = "Hello World";

                using (var stream = File.Open(@"C:\Users\dkaushikl\Documents\Visual Studio 2015\Projects\DemoAmazon\DemoAmazon\pdf\pdf.pdf", FileMode.Open)) builder.Attachments.Add(@"C:\Users\dkaushikl\Documents\Visual Studio 2015\Projects\DemoAmazon\DemoAmazon\pdf\pdf.pdf", stream);

                using (var stream = File.Open(@"C:\Users\dkaushikl\Documents\Visual Studio 2015\Projects\DemoAmazon\DemoAmazon\pdf\pdf-sample.pdf", FileMode.Open)) builder.Attachments.Add(@"C:\Users\dkaushikl\Documents\Visual Studio 2015\Projects\DemoAmazon\DemoAmazon\pdf\pdf-sample.pdf", stream);

                message.Body = builder.ToMessageBody();
                message.WriteTo(messageStream);

                var request = new SendRawEmailRequest
                {
                    RawMessage = new RawMessage { Data = messageStream }
                };
                client.SendRawEmail(request);
            }
        }



        private void SendSmtpEmail()
        {
            using (var client = new SmtpClient(Host, Port))
            {
                client.Credentials = new System.Net.NetworkCredential(SmtpUsername, SmtpPassword);
                client.EnableSsl = true;
                var objMailMessage = new MailMessage
                {
                    From = new MailAddress(SenderAddress),
                    Subject = "this is test subject",
                    Body = "this is test body"
                };
                objMailMessage.To.Add(ReceiverAddress);
                client.Send(objMailMessage);
                Console.WriteLine("Email sent!");
            }
        }

    }
}

//using (var clients = new AmazonSimpleNotificationServiceClient(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
//{
//    var test = clients.CreateTopic("bounce-complaint-topic");
//    var objSubscribeRequest = new SubscribeRequest
//    {
//        TopicArn = test.TopicArn,
//        Endpoint = "http://yourwebsite.com/Client/Bounce",
//        Protocol = "http"
//    };
//    clients.Subscribe(objSubscribeRequest);
//}

//using (var clients = new AmazonSimpleNotificationServiceClient(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
//{
//    var test = clients.CreateTopic("delivery-complaint-topic");
//    var objSubscribeRequest = new SubscribeRequest
//    {
//        TopicArn = test.TopicArn,
//        Endpoint = "http://yourwebsite.com/Client/Delivery",
//        Protocol = "http"
//    };
//    clients.Subscribe(objSubscribeRequest);
//}

//using (var clients = new AmazonSimpleNotificationServiceClient(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
//{
//    var test = clients.CreateTopic("complaint-complaint-topic");
//    var objSubscribeRequest = new SubscribeRequest
//    {
//        TopicArn = test.TopicArn,
//        Endpoint = "http://yourwebsite.com/Client/Complaint",
//        Protocol = "http"
//    };
//    clients.Subscribe(objSubscribeRequest);
//}