using System.Text.Json;
using AlMostashar.Application.Common.Exceptions;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LegalAi.Commands.AskLegalAi;
using AlMostashar.Application.Features.LegalAi.DTOs;
using Moq;

namespace AlMostashar.Application.Tests;

public class LegalAiIntegrationTests
{
    [Fact]
    public void LegalAiChatResponse_DeserializesAnswerParts()
    {
        const string json = """
        {
          "answer_mode": "grounded",
          "final_answer": "جواب مختصر.",
          "answer_parts": {
            "intro": "مقدمة قصيرة.",
            "section_title": "أهم الأحكام:",
            "bullets": ["نقطة أولى", "نقطة ثانية"],
            "legal_basis": "استندت الإجابة إلى المادة 54.",
            "note": null
          },
          "is_legal_question": true,
          "is_supported_by_internal_sources": true,
          "is_out_of_internal_corpus": false,
          "sources": [],
          "llm": {"called": true, "succeeded": true, "provider": "gemini", "model": "gemini-2.5-flash"}
        }
        """;

        var response = JsonSerializer.Deserialize<LegalAiChatResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(response);
        Assert.NotNull(response!.AnswerParts);
        Assert.Equal("مقدمة قصيرة.", response.AnswerParts!.Intro);
        Assert.Equal("أهم الأحكام:", response.AnswerParts.SectionTitle);
        Assert.Equal(2, response.AnswerParts.Bullets.Count);
        Assert.Equal("استندت الإجابة إلى المادة 54.", response.AnswerParts.LegalBasis);
    }

    [Fact]
    public async Task AskLegalAiCommandHandler_TrimsQuery_AndReturnsAiResponse()
    {
        var client = new Mock<ILegalAiClient>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(7);

        client
            .Setup(x => x.AskAsync("ما ضمانات الحرية الشخصية؟", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LegalAiChatResponse
            {
                AnswerMode = "grounded",
                FinalAnswer = "إجابة قانونية مختصرة."
            });

        var handler = new AskLegalAiCommandHandler(client.Object, currentUser.Object);

        var result = await handler.Handle(
            new AskLegalAiCommand("  ما ضمانات الحرية الشخصية؟  "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("grounded", result.Value!.AnswerMode);
        client.Verify(x => x.AskAsync("ما ضمانات الحرية الشخصية؟", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AskLegalAiCommandHandler_ReturnsUnavailableError_WhenClientFails()
    {
        var client = new Mock<ILegalAiClient>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(7);

        client
            .Setup(x => x.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LegalAiServiceUnavailableException("failed"));

        var handler = new AskLegalAiCommandHandler(client.Object, currentUser.Object);

        var result = await handler.Handle(new AskLegalAiCommand("سؤال قانوني"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("LegalAi.Unavailable", result.Error!.Code);
        Assert.Equal("Legal AI service is temporarily unavailable.", result.Error.Message);
    }
}
