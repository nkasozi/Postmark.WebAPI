using Xunit;
using PostMark.Akka.Actors;
using System;
using System.Collections.Generic;
using System.Text;
using Postmark.WebAPI.Models;
using Akka.Actor;
using FluentAssertions;
using Postmark.BussinessLogic.Interfaces;
using Postmark.BussinessLogic;
using System.Linq;
using AutoMapper;

namespace PostMark.Akka.Actors.Tests
{
    public class BussinessLogicActorTests : BaseTest
    {
        //test single valid email
        [Fact()]
        public void Given_ValidSingleEmail_ExpectSuccess()
        {
            //arrange
            var emailSendingActor = CreateTestProbe("TestEmailSendingActor");
            var failedEmailsActor = CreateTestProbe("TestFailedEmailsActor");

            AutoMapperConfig.Init();
            IEmailBussinessRule unsubscribeRule = new UnsubscribeRule();
            IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator().WithRule(unsubscribeRule);

            var actor = Sys.ActorOf(BussinessLogicActor.Create(emailSendingActor, failedEmailsActor, rulesEvaluator));

            var ID = "1234";
            var request = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = new SingleEmail
                {
                    To = "test@test.com",
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            };

            //act
            var answer = actor.Ask<SingleEmailResult>(request).Result;

            //assert
            answer.Should().NotBeNull();
            answer.ErrorCode.Should().Be(OpResultCode.SUCCCESS);
            answer.Message.Should().NotBeNullOrEmpty();
            answer.To.Should().NotBeNullOrEmpty();
            answer.MessageID.Should().NotBeNullOrEmpty();
            answer.UniqueEmailID.Should().BeSameAs(ID);
        }

        //test bulk valid email
        [Fact()]
        public void Given_ValidBulkEmail_ExpectSuccess()
        {
            //arrange
            var emailSendingActor = CreateTestProbe("TestEmailSendingActor");
            var failedEmailsActor = CreateTestProbe("TestFailedEmailsActor");
            var ID = "1234";
            var ID2 = "12345";
            var ID3 = "12346";
            var testEmail = "test@test.com";

            AutoMapperConfig.Init();
            IEmailBussinessRule unsubscribeRule = new UnsubscribeRule();
            IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator().WithRule(unsubscribeRule);

            var actor = Sys.ActorOf(BussinessLogicActor.Create(emailSendingActor, failedEmailsActor, rulesEvaluator));



            var testEmailList = new List<SingleEmail>();

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test 2",
                    UniqueEmailID = ID2
                }
             );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test 3",
                    UniqueEmailID = ID3
                }
            );

            var request = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = new BulkEmail
                {
                    //quick way to create a deep copy of something
                    Emails = Mapper.Map<List<SingleEmail>>(testEmailList)
                }
            };

            //act
            var answer = actor.Ask<BulkEmailResult>(request).Result;

            //assert
            answer.Should().NotBeNull();
            answer.Results.Count.Should().Be(testEmailList.Count);

            //check that each property is filled
            foreach (var emailResult in answer.Results)
            {
                emailResult.Should().NotBeNull();
                emailResult.ErrorCode.Should().Be(OpResultCode.SUCCCESS);
                emailResult.Message.Should().NotBeNullOrEmpty();
                emailResult.To.Should().NotBeNullOrEmpty();
                emailResult.MessageID.Should().NotBeNullOrEmpty();
                emailResult.UniqueEmailID.Should().NotBeNullOrEmpty();
                Assert.Contains(testEmailList, x => x.UniqueEmailID == emailResult.UniqueEmailID);
            }

        }

        //test bulk invalid email
        [Fact()]
        public void Given_InvalidBulkEmail_ExpectFailure()
        {
            //arrange
            var emailSendingActor = CreateTestProbe("TestEmailSendingActor");
            var failedEmailsActor = CreateTestProbe("TestFailedEmailsActor");
            var emailUnderTest = "test@test.com";
            var ID = "1234";
            var ID2 = "12345";
            var ID3 = "12346";

            AutoMapperConfig.Init();
            UnsubscribeRule unsubscribeRule = new UnsubscribeRule();
            unsubscribeRule.UnbsubscribedEmails.Add(emailUnderTest);
            IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator().WithRule(unsubscribeRule);

            var actor = Sys.ActorOf(BussinessLogicActor.Create(emailSendingActor, failedEmailsActor, rulesEvaluator));



            var testEmailList = new List<SingleEmail>();

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = emailUnderTest,
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = emailUnderTest,
                    HtmlBody = "Test 2",
                    UniqueEmailID = ID2
                }
             );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = emailUnderTest,
                    HtmlBody = "Test 3",
                    UniqueEmailID = ID3
                }
            );


            var request = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = new BulkEmail
                {
                    //quick way to create a deep copy of something
                    Emails = Mapper.Map<List<SingleEmail>>(testEmailList)
                }
            };

            //act
            var answer = actor.Ask<BulkEmailResult>(request).Result;

            //assert
            answer.Should().NotBeNull();
            answer.Results.Count.Should().Be(testEmailList.Count);

            //check that each property is filled
            foreach (var emailResult in answer.Results)
            {
                emailResult.Should().NotBeNull();
                emailResult.ErrorCode.Should().NotBe(OpResultCode.SUCCCESS);
                emailResult.Message.Should().NotBeNullOrEmpty();
                emailResult.Message.Should().Be(UnsubscribeRule.ERROR_MESSAGE);
                emailResult.To.Should().NotBeNullOrEmpty();
                emailResult.MessageID.Should().NotBeNullOrEmpty();
                emailResult.UniqueEmailID.Should().NotBeNullOrEmpty();
                Assert.Contains(testEmailList, x => x.UniqueEmailID == emailResult.UniqueEmailID);
            }

        }

        //test bulk with mixture of valid and invalid emails
        [Fact()]
        public void Given_BulkEmailWithBothValidAndInvalid_ExpectMixedResults()
        {
            //arrange
            var emailSendingActor = CreateTestProbe("TestEmailSendingActor");
            var failedEmailsActor = CreateTestProbe("TestFailedEmailsActor");
            var emailUnderTest = "test@test.com";
            var ID = "1234";
            var ID2 = "12345";
            var ID3 = "12346";

            AutoMapperConfig.Init();
            UnsubscribeRule unsubscribeRule = new UnsubscribeRule();
            unsubscribeRule.UnbsubscribedEmails.Add(emailUnderTest);
            IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator().WithRule(unsubscribeRule);

            var actor = Sys.ActorOf(BussinessLogicActor.Create(emailSendingActor, failedEmailsActor, rulesEvaluator));

            var testEmailList = new List<SingleEmail>();

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = emailUnderTest,
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = "test2@yahoo.com",
                    HtmlBody = "Test 2",
                    UniqueEmailID = ID2
                }
             );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = emailUnderTest,
                    HtmlBody = "Test 3",
                    UniqueEmailID = ID3
                }
            );


            var request = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = new BulkEmail
                {
                    //quick way to create a deep copy of something
                    Emails = Mapper.Map<List<SingleEmail>>(testEmailList)
                }
            };

            //act
            var answer = actor.Ask<BulkEmailResult>(request).Result;

            //assert
            answer.Should().NotBeNull();
            answer.Results.Count.Should().Be(testEmailList.Count);

            answer.Results.Find(i=>i.UniqueEmailID==ID).Should().NotBeNull();
            answer.Results.Find(i=>i.UniqueEmailID==ID).ErrorCode.Should().NotBe(OpResultCode.SUCCCESS);
            answer.Results.Find(i=>i.UniqueEmailID==ID).Message.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID).Message.Should().Be(UnsubscribeRule.ERROR_MESSAGE);
            answer.Results.Find(i=>i.UniqueEmailID==ID).To.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID).MessageID.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID).UniqueEmailID.Should().NotBeNullOrEmpty();

            answer.Results.Find(i=>i.UniqueEmailID==ID2).Should().NotBeNull();
            answer.Results.Find(i=>i.UniqueEmailID==ID2).ErrorCode.Should().Be(OpResultCode.SUCCCESS);
            answer.Results.Find(i=>i.UniqueEmailID==ID2).Message.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID2).To.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID2).MessageID.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID2).UniqueEmailID.Should().NotBeNullOrEmpty();

            answer.Results.Find(i=>i.UniqueEmailID==ID3).Should().NotBeNull();
            answer.Results.Find(i=>i.UniqueEmailID==ID3).ErrorCode.Should().NotBe(OpResultCode.SUCCCESS);
            answer.Results.Find(i=>i.UniqueEmailID==ID3).Message.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID3).Message.Should().Be(UnsubscribeRule.ERROR_MESSAGE);
            answer.Results.Find(i=>i.UniqueEmailID==ID3).To.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID3).MessageID.Should().NotBeNullOrEmpty();
            answer.Results.Find(i=>i.UniqueEmailID==ID3).UniqueEmailID.Should().NotBeNullOrEmpty();

        }

        //test single valid email
        [Fact()]
        public void Given_InvalidSingleEmail_ExpectFailure()
        {
            //arrange
            var emailSendingActor = CreateTestProbe("TestEmailSendingActor");
            var failedEmailsActor = CreateTestProbe("TestFailedEmailsActor");

            AutoMapperConfig.Init();
            UnsubscribeRule unsubscribeRule = new UnsubscribeRule();
            unsubscribeRule.UnbsubscribedEmails.Add("test@test.com");
            IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator().WithRule(unsubscribeRule);

            var actor = Sys.ActorOf(BussinessLogicActor.Create(emailSendingActor, failedEmailsActor, rulesEvaluator));

            var ID = "1234";
            var request = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = new SingleEmail
                {
                    To = "test@test.com",
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            };

            //act
            var answer = actor.Ask<SingleEmailResult>(request).Result;

            //assert
            answer.Should().NotBeNull();
            answer.ErrorCode.Should().Be(OpResultCode.FAILURE);
            answer.Message.Should().NotBeNullOrEmpty();
            answer.To.Should().NotBeNullOrEmpty();
            answer.MessageID.Should().NotBeNullOrEmpty();
            answer.UniqueEmailID.Should().BeSameAs(ID);
        }

        //test bulk with many rules
        //test bulk with mixture of valid and invalid emails
        [Fact()]
        public void Given_BulkEmailWithBothValidAndInvalidAndManyRules_ExpectMixedResults()
        {
            //arrange
            var emailSendingActor = CreateTestProbe("TestEmailSendingActor");
            var failedEmailsActor = CreateTestProbe("TestFailedEmailsActor");
            var testEmail = "test@test.com";
            var ID = "1234";
            var ID2 = "12345";
            var ID3 = "12346";

            AutoMapperConfig.Init();

            var unsubscribeRule = new UnsubscribeRule();
            unsubscribeRule.UnbsubscribedEmails.Add(testEmail);
            var validateToEmailAddressRule = new ValidateToEmailAddressRule();

            IEmailBussinessRule[] emailBussinessRules =
            {
                unsubscribeRule,
                validateToEmailAddressRule
            };

            IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator()
                                                       .WithRules(emailBussinessRules);

            var actor = Sys.ActorOf(BussinessLogicActor.Create(emailSendingActor, failedEmailsActor, rulesEvaluator));

            var testEmailList = new List<SingleEmail>();

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = "test2@yahoo.com",
                    HtmlBody = "Test 2",
                    UniqueEmailID = ID2
                }
             );

            testEmailList.Add
            (
                new SingleEmail
                {
                    To = "test",
                    HtmlBody = "Test 3",
                    UniqueEmailID = ID3
                }
            );


            var request = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = new BulkEmail
                {
                    //quick way to create a deep copy of something
                    Emails = Mapper.Map<List<SingleEmail>>(testEmailList)
                }
            };

            //act
            var answer = actor.Ask<BulkEmailResult>(request).Result;

            //assert
            answer.Should().NotBeNull();
            answer.Results.Count.Should().Be(testEmailList.Count);

            answer.Results.Find(i => i.UniqueEmailID == ID).Should().NotBeNull();
            answer.Results.Find(i => i.UniqueEmailID == ID).ErrorCode.Should().NotBe(OpResultCode.SUCCCESS);
            answer.Results.Find(i => i.UniqueEmailID == ID).Message.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID).Message.Should().Be(UnsubscribeRule.ERROR_MESSAGE);
            answer.Results.Find(i => i.UniqueEmailID == ID).To.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID).MessageID.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID).UniqueEmailID.Should().NotBeNullOrEmpty();

            answer.Results.Find(i => i.UniqueEmailID == ID2).Should().NotBeNull();
            answer.Results.Find(i => i.UniqueEmailID == ID2).ErrorCode.Should().Be(OpResultCode.SUCCCESS);
            answer.Results.Find(i => i.UniqueEmailID == ID2).Message.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID2).To.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID2).MessageID.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID2).UniqueEmailID.Should().NotBeNullOrEmpty();

            answer.Results.Find(i => i.UniqueEmailID == ID3).Should().NotBeNull();
            answer.Results.Find(i => i.UniqueEmailID == ID3).ErrorCode.Should().NotBe(OpResultCode.SUCCCESS);
            answer.Results.Find(i => i.UniqueEmailID == ID3).Message.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID3).Message.Should().Be(ValidateToEmailAddressRule.ERROR_MESSAGE);
            answer.Results.Find(i => i.UniqueEmailID == ID3).To.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID3).MessageID.Should().NotBeNullOrEmpty();
            answer.Results.Find(i => i.UniqueEmailID == ID3).UniqueEmailID.Should().NotBeNullOrEmpty();

        }
    }
}