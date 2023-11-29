using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Moq;
using FlashcardProject.Controllers;
using FlashcardProject.DAL;
using FlashcardProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;


namespace XunitTestFlashcardProject.Controllers;

public class FlashcardControllerTests
{
    private readonly Mock<IFlashcardRepository> _mockFlashcardRepository;
    private readonly Mock<ILogger<FlashcardController>> _mockLogger;
    private readonly FlashcardController _flashcardController;
    private readonly ITestOutputHelper _output;
    public FlashcardControllerTests(ITestOutputHelper output)
    {
        _mockFlashcardRepository = new Mock<IFlashcardRepository>();
        _mockLogger = new Mock<ILogger<FlashcardController>>();
        _flashcardController = new FlashcardController(_mockFlashcardRepository.Object, _mockLogger.Object);
        _output = output;
    }

    [Fact]
    public async Task TestGetAllNoFlashcardsFound()
    {
        // arrange
        _mockFlashcardRepository.Setup(repo => repo.GetAll()).ReturnsAsync((IEnumerable<Flashcard>?)null!);

        // act
        var result = await _flashcardController.GetAll();

        //assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Flashcard list not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetAllOk()
    {
        // arrange
        var creationDate = DateTime.Now;
        var testDeckId = 1;
        var flashcardList = new List<Flashcard>()
        {
            new Flashcard
            {
                FlashcardId = 1,
                Question = "TestFlashcard1",
                Answer = "Dummy flashcard1",
                CreationDate = creationDate,
                DeckId = testDeckId
            },
            new Flashcard
            {
                FlashcardId = 2,
                Question = "TestFlashcard2",
                Answer = "Dummy flashcard2",
                CreationDate = creationDate,
                DeckId = testDeckId
            },
            new Flashcard
            {
                FlashcardId = 3,
                Question = "TestFlashcard3",
                Answer = "Dummy flashcard3",
                CreationDate = creationDate,
                DeckId = testDeckId
            }
        };
        _mockFlashcardRepository.Setup(repo => repo.GetAll()).ReturnsAsync(flashcardList);

        //act
        var result = await _flashcardController.GetAll();

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultFlashcards = Assert.IsType<List<Flashcard>>(okResult.Value);
        Assert.Equal(flashcardList.Count, resultFlashcards.Count);

        // each attribute of the test flashcardList and the one returned from the controller are compared to each other
        for (int i = 0; i < flashcardList.Count; i++)
        {
            Assert.Equal(flashcardList[i].FlashcardId, resultFlashcards[i].FlashcardId);
            Assert.Equal(flashcardList[i].Question, resultFlashcards[i].Question);
            Assert.Equal(flashcardList[i].Answer, resultFlashcards[i].Answer);
            Assert.Equal(flashcardList[i].CreationDate, resultFlashcards[i].CreationDate);
            Assert.Equal(flashcardList[i].DeckId, resultFlashcards[i].DeckId);
        }
    }

    [Fact]
    public async Task TestGetFlashcardsByDeckIdFlashcardsNotFound()
    {
        //arrange
        int testDeckId = 1;
        _mockFlashcardRepository.Setup(repo => repo.GetFlashcardsByDeckId(testDeckId)).ReturnsAsync((IEnumerable<Flashcard>?)null!);

        // act
        var result = await _flashcardController.GetFlashcardsByDeckId(testDeckId);

        // assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Flashcard list not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetFlashcardsByDeckIdOk()
    {
        // arrange
        var creationDate = DateTime.Now;
        var testDeckId = 1;
        var flashcardList = new List<Flashcard>()
        {
            new Flashcard
            {
                FlashcardId = 1,
                Question = "TestFlashcard1",
                Answer = "Dummy flashcard1",
                CreationDate = creationDate,
                DeckId = testDeckId
            },
            new Flashcard
            {
                FlashcardId = 2,
                Question = "TestFlashcard2",
                Answer = "Dummy flashcard2",
                CreationDate = creationDate,
                DeckId = testDeckId

            },
            new Flashcard
            {
                FlashcardId = 3,
                Question = "TestFlashcard3",
                Answer = "Dummy flashcard3",
                CreationDate = creationDate,
                DeckId = testDeckId
           },
            new Flashcard
            {
                FlashcardId = 4,
                Question = "TestFlashcard4",
                Answer = "Dummy flashcard4",
                CreationDate = creationDate,
                DeckId = testDeckId
            }
        };
        _mockFlashcardRepository.Setup(repo => repo.GetFlashcardsByDeckId(testDeckId)).ReturnsAsync(flashcardList);

        // act
        var result = await _flashcardController.GetFlashcardsByDeckId(testDeckId);
        Assert.NotNull(result);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultFlashcards = Assert.IsType<List<Flashcard>>(okResult.Value);
        Assert.Equal(4, resultFlashcards.Count);

        // id starts with 0, but the list indexing start with 0
        for (int i = 0; i < resultFlashcards.Count; i++)
        {
            Assert.IsType<int>(resultFlashcards[i].FlashcardId);
            Assert.Equal(i + 1, resultFlashcards[i].FlashcardId);

            Assert.IsType<string>(resultFlashcards[i].Question);
            Assert.Equal("TestFlashcard" + (i + 1), resultFlashcards[i].Question);

            Assert.IsType<string>(resultFlashcards[i].Answer);
            Assert.Equal("Dummy flashcard" + (i + 1), resultFlashcards[i].Answer);

            Assert.IsType<int>(resultFlashcards[i].DeckId);
            Assert.Equal(testDeckId, resultFlashcards[i].DeckId);
        }
    }
    [Fact]
    public async Task TestCreateInvalidFlashcard()
    {
        // act 
        var result = await _flashcardController.Create(It.IsAny<int>(), null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid Flashcard data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestCreateOk()
    {
        // arrange
        var testFlashcard = new Flashcard
        {
            FlashcardId = 1,
            Question = "TestFlashcard1",
            Answer = "Dummy flashcard1",
            CreationDate = DateTime.Now
        };

        _mockFlashcardRepository.Setup(repo => repo.Create(testFlashcard)).ReturnsAsync(true);

        // act
        var result = await _flashcardController.Create(1, testFlashcard);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Flashcard created successfully", responseMessage);
    }

    [Fact]
    public async Task TestCreateNotOk()
    {
        //arrange
        var testFlashcard = new Flashcard
        {
            FlashcardId = 1,
            Question = "TestFlashcard1",
            Answer = "Dummy flashcard1",
            CreationDate = DateTime.Now
        };

        _mockFlashcardRepository.Setup(repo => repo.Create(testFlashcard)).ReturnsAsync(false);

        //act
        var result = await _flashcardController.Create(1, testFlashcard);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Flashcard creation failed", responseMessage);
    }

    [Fact]
    public async Task TestGetFlashcardByIdInvalidFlashcard()
    {
        // arrange
        int testId = 1;
        _mockFlashcardRepository.Setup(repo => repo.GetFlashcardById(testId)).ReturnsAsync((Flashcard?)null);

        // act
        var result = await _flashcardController.GetFlashcardbyId(testId);

        // assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Flashcard not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetFlashcardByIdOk()
    {
        // arrange
        var testFlashcard = new Flashcard
        {
            FlashcardId = 1,
            Question = "TestFlashcard1",
            Answer = "Dummy flashcard1",
            CreationDate = DateTime.Now
        };

        int testId = 1;

        _mockFlashcardRepository.Setup(repo => repo.GetFlashcardById(testId)).ReturnsAsync(testFlashcard);

        // act
        var result = await _flashcardController.GetFlashcardbyId(testId);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFlashcard = Assert.IsType<Flashcard>(okResult.Value);
        Assert.Equal(testFlashcard, returnedFlashcard);
    }

    [Fact]
    public async Task TestUpdateInvalidFlashcard()
    {
        //arrange
        _mockFlashcardRepository.Setup(repo => repo.Update(null!)).ReturnsAsync(false);

        //act
        var result = await _flashcardController.Update(null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid Flashcard data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestUpdateOk()
    {
        //arrange
        var testFlashcard = new Flashcard
        {
            FlashcardId = 1,
            Question = "TestFlashcard1",
            Answer = "Dummy flashcard1",
            CreationDate = DateTime.Now
        };

        _mockFlashcardRepository.Setup(repo => repo.Update(testFlashcard)).ReturnsAsync(true);

        //act
        var result = await _flashcardController.Update(testFlashcard);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Flashcard #" + testFlashcard.FlashcardId + " updated successfully", responseMessage);
    }

    [Fact]
    public async Task TestUpdateNotOk()
    {
        // Arrange
        var testFlashcard = new Flashcard
        {
            FlashcardId = 1,
            Question = "TestFlashcard1",
            Answer = "Dummy flashcard1",
            CreationDate = DateTime.Now
        };

        _mockFlashcardRepository.Setup(repo => repo.Update(testFlashcard)).ReturnsAsync(false);

        // Act
        var result = await _flashcardController.Update(testFlashcard);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Flashcard update failed", responseMessage);
    }

    [Fact]
    public async Task TestDeleteFlashcardNotOk()
    {
        //arrange
        var testId = 1;
        _mockFlashcardRepository.Setup(repo => repo.Delete(testId)).ReturnsAsync(false);

        //act
        var result = await _flashcardController.DeleteFlashcard(testId);

        //assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var badRequestMessage = Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Flashcard deletion failed", badRequestMessage);
    }

    [Fact]
    public async Task TestDeleteFlashcardOk()
    {
        //arrange
        var testId = 1;
        _mockFlashcardRepository.Setup(repo => repo.Delete(testId)).ReturnsAsync(true);

        //act
        var result = await _flashcardController.DeleteFlashcard(testId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Flashcard #" + testId.ToString() + " deleted successfully", responseMessage);
    }
}
