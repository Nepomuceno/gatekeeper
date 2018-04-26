using System;
using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace gatekeeper {
    public class SmsSender {

        String AccountSid = ConfigurationManager.AppSettings["AccountSid"];
        String AuthToken = ConfigurationManager.AppSettings["TwilioAuthToken"];

        public void SendSms (String content) {

            TwilioClient.Init (AccountSid, AuthToken);

            var message = MessageResource.Create (
                to : new PhoneNumber ("+447878654221"),
                from : new PhoneNumber ("+447480486534"),
                body : content);

        }
    }
}