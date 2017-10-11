using System;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon;

namespace DemoAmazon
{
    public class Sns
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
        public static RegionEndpoint EndPoint { get; set; } = RegionEndpoint.USWest2;
        public static void Main()
        {
            var sns = new AmazonSimpleNotificationServiceClient(AwsAccessKeyId, AwsSecretAccessKey, EndPoint);
            var emailAddress = "email address";
            while (string.IsNullOrEmpty(emailAddress))
            {
                Console.Write("Please enter an email address to use: ");
                emailAddress = Console.ReadLine();
            }

            try
            {
                // Create topic
                Console.WriteLine("Creating topic...");
                var topicArn = sns.CreateTopic(new CreateTopicRequest
                {
                    Name = "SampleSNSTopic"
                }).TopicArn;

                // Set display name to a friendly value
                Console.WriteLine();
                Console.WriteLine("Setting topic attributes...");
                sns.SetTopicAttributes(new SetTopicAttributesRequest
                {
                    TopicArn = topicArn,
                    AttributeName = "DisplayName",
                    AttributeValue = "Sample Notifications"
                });

                // List all topics
                Console.WriteLine();
                Console.WriteLine("Retrieving all topics...");
                var listTopicsRequest = new ListTopicsRequest();
                ListTopicsResponse listTopicsResponse;
                do
                {
                    listTopicsResponse = sns.ListTopics(listTopicsRequest);
                    foreach (var topic in listTopicsResponse.Topics)
                    {
                        Console.WriteLine(" Topic: {0}", topic.TopicArn);

                        // Get topic attributes
                        var topicAttributes = sns.GetTopicAttributes(new GetTopicAttributesRequest
                        {
                            TopicArn = topic.TopicArn
                        }).Attributes;
                        if (topicAttributes.Count > 0)
                        {
                            Console.WriteLine(" Topic attributes");
                            foreach (var topicAttribute in topicAttributes)
                            {
                                Console.WriteLine(" -{0} : {1}", topicAttribute.Key, topicAttribute.Value);
                            }
                        }
                        Console.WriteLine();
                    }
                    listTopicsRequest.NextToken = listTopicsResponse.NextToken;
                } while (listTopicsResponse.NextToken != null);

                // Subscribe an endpoint - in this case, an email address
                Console.WriteLine();
                Console.WriteLine("Subscribing email address {0} to topic...", emailAddress);
                sns.Subscribe(new SubscribeRequest
                {
                    TopicArn = topicArn,
                    Protocol = "email",
                    Endpoint = emailAddress
                });

                // When using email, recipient must confirm subscription
                Console.WriteLine();
                Console.WriteLine("Please check your email and press enter when you are subscribed...");
                Console.ReadLine();

                // Publish message
                Console.WriteLine();
                Console.WriteLine("Publishing message to topic...");
                sns.Publish(new PublishRequest
                {
                    Subject = "Test",
                    Message = "Testing testing 1 2 3",
                    TopicArn = topicArn
                });

                // Verify email receieved
                Console.WriteLine();
                Console.WriteLine("Please check your email and press enter when you receive the message...");
                Console.ReadLine();

                // Delete topic
                Console.WriteLine();
                Console.WriteLine("Deleting topic...");
                sns.DeleteTopic(new DeleteTopicRequest
                {
                    TopicArn = topicArn
                });
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                Console.WriteLine("Caught Exception: " + ex.Message);
                Console.WriteLine("Response Status Code: " + ex.StatusCode);
                Console.WriteLine("Error Code: " + ex.ErrorCode);
                Console.WriteLine("Error Type: " + ex.ErrorType);
                Console.WriteLine("Request ID: " + ex.RequestId);
            }
             
            Console.WriteLine();
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
