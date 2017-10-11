using System;
using Amazon.Route53.Model;
using Amazon;
using System.Collections.Generic;
using Amazon.Route53;

namespace DemoAmazon
{
    internal class Route53
    {
    
        private const string Zoneid = "Your zone id";
        private const string Fqdn = "Your record name";
        private const string IpAddress = "Your IP Address";

        private const string AwsAccessKeyId = "Access Key ID";
        private const string AwsSecretAccessKey = "Access Secret Key ID";

        private static readonly RegionEndpoint EndPoint = RegionEndpoint.USWest2;

        private static void Main()
        {
            // Init the client
            var r53Client = new AmazonRoute53Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint);

            CreateHostedZone(r53Client, "kdh.blake.ly");
            CreateRecordSet(r53Client);
            var objGetHostedZones = GetHostedZones(r53Client);
            var objResourceRecordSets = GetResourceRecordSets(r53Client);

            Console.WriteLine(objGetHostedZones);
            Console.WriteLine(objResourceRecordSets);
        }

        public static ListHostedZonesResponse GetHostedZones(IAmazonRoute53 r53Client)
        {
            var request = new ListHostedZonesRequest { MaxItems = "100" };
            var response = r53Client.ListHostedZones(request);
            return response;
        }

        public static ListResourceRecordSetsResponse GetResourceRecordSets(IAmazonRoute53 r53Client)
        {
            var request = new ListResourceRecordSetsRequest
            {
                HostedZoneId = Zoneid,
                StartRecordName = Fqdn,
                StartRecordType = "A",
                MaxItems = "100"
            };
            var response = r53Client.ListResourceRecordSets(request);
            return response;
        }

        private static void CreateHostedZone(IAmazonRoute53 r53Client, string domainName)
        {
            var zoneRequest = new CreateHostedZoneRequest
            {
                Name = domainName,
                CallerReference = "testingss"
            };

            var zoneResponse = r53Client.CreateHostedZone(zoneRequest);
            var recordSet = new ResourceRecordSet
            {
                Name = domainName,
                TTL = 60,
                Type = RRType.A,
                ResourceRecords = new List<ResourceRecord> { new ResourceRecord { Value = IpAddress } }
            };

            var change1 = new Change
            {
                ResourceRecordSet = recordSet,
                Action = ChangeAction.CREATE
            };

            var changeBatch = new ChangeBatch
            {
                Changes = new List<Change> { change1 }
            };

            var recordsetRequest = new ChangeResourceRecordSetsRequest
            {
                HostedZoneId = zoneResponse.HostedZone.Id,
                ChangeBatch = changeBatch
            };

            var recordsetResponse = r53Client.ChangeResourceRecordSets(recordsetRequest);
            var changeRequest = new GetChangeRequest
            {
                Id = recordsetResponse.ChangeInfo.Id
            };
            Console.WriteLine(changeRequest);

        }

        private static void CreateRecordSet(IAmazonRoute53 r53Client)
        {
            var objRecord = new ResourceRecord { Value = "Your IP Address" };

            var objRecordSet = new ResourceRecordSet
            {
                Name = Fqdn,
                Type = "A",
                TTL = 300
            };
            objRecordSet.ResourceRecords.Add(objRecord);

            var objChange = new Change
            {
                Action = ChangeAction.UPSERT,
                ResourceRecordSet = objRecordSet
            };

            var objChangeList = new List<Change> { objChange };

            var objChangeBatch = new ChangeBatch { Changes = objChangeList };

            var objRequest = new ChangeResourceRecordSetsRequest
            {
                HostedZoneId = Zoneid,
                ChangeBatch = objChangeBatch
            };

            r53Client.ChangeResourceRecordSets(objRequest);
        }
    }
}
