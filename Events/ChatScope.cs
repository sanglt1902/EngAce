﻿using Entities;
using Gemini;
using System.Text;
using static Gemini.DTO.ChatRequest;

namespace Events
{
    public static class ChatScope
    {
        public static async Task<string> GenerateAnswer(string apiKey, Conversation conversation)
        {
            if (!conversation.ChatHistory.Any())
            {
                conversation.ChatHistory.AddRange(InitPrompts());
            }

            var request = new Request
            {
                Contents = conversation.ChatHistory.Select(message => new Content
                {
                    Role = message.FromUser ? "user" : "model",
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            Text = message.Message,
                        }
                    }
                })
                .ToList()
            };

            var question = new Content
            {
                Role = "user",
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = conversation.Question
                    }
                }
            };

            request.Contents.Add(question);

            return await Generator.Generate(apiKey, request);
        }

        private static List<Conversation.History> InitPrompts()
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine("Bạn là EngAce, một AI được tạo ra mới mục tiêu hỗ trợ người dùng học tiếng Anh một cách hiệu quả. ");
            promptBuilder.Append($"Tôi là một người đang học tiếng Anh, mong muốn của tôi là cải thiện tất cả những kỹ năng tiếng Anh, và học hỏi thêm kinh nghiệm để vận dụng tiếng Anh hiệu quả vào công việc lẫn học tập. ");
            promptBuilder.AppendLine($"Nhiệm vụ của bạn là giúp tôi giải đáp những thắc mắc liên quan đến việc học tiếng Anh và tư vấn phương pháp học tiếng Anh hiệu quả, bạn cũng có thể giúp tôi thực hiện tra cứu nếu cần thiết. ");
            promptBuilder.Append("Bạn chỉ được phép trả lời những câu hỏi liên quan đến việc học tiếng Anh, ngoài ra không được phép trả lời. Nếu bạn cảm thấy câu hỏi của tôi không rõ ràng, bạn có thể hỏi tôi để làm rõ ý định của tôi đối với câu hỏi. ");
            promptBuilder.Append("Câu trả lời của bạn phải ngắn gọn và dễ hiểu ngay cả với những người mới học tiếng Anh, bạn cũng có thể cung cấp một số ví dụ minh họa nếu cần thiết. ");
            promptBuilder.Append("Cách nói chuyện của bạn phải thật thân thiện và mang cảm giác gần gũi, bởi vì bạn chính là bạn đồng hành của tôi trong quá trình tôi học tiếng Anh.");
            promptBuilder.AppendLine("Nếu bạn hiểu lời nói của tôi thì hãy tự giới thiệu bản thân, và chúng ta sẽ bắt đầu cuộc trò chuyện.");

            var prompt = new Conversation.History()
            {
                FromUser = true,
                Message = promptBuilder.ToString()
            };

            var botReply = new Conversation.History()
            {
                FromUser = false,
                Message = "Xin chào! Tớ là EngAce, một AI được tạo ra mới mục tiêu hỗ trợ bạn học tiếng Anh một cách hiệu quả. Tớ sẽ là bạn đồng hành của bạn trong thời gian bạn học tiếng Anh. Nếu bạn có thắc mắc liên quan đến việc học tiếng Anh thì hãy hỏi tớ nhé!"
            };

            return new List<Conversation.History>() { prompt, botReply };
        }
    }
}
