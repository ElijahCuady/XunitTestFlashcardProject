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

public class DeckControllerTests
{
    private readonly Mock<IDeckRepository> _mockDeckRepository;
    private readonly Mock<ILogger<DeckController>> _mockLogger;
    private readonly DeckController _deckController;
    private readonly ITestOutputHelper _output;
    public DeckControllerTests(ITestOutputHelper output)
    {
        _mockDeckRepository = new Mock<IDeckRepository>();
        _mockLogger = new Mock<ILogger<DeckController>>();
        _deckController = new DeckController(_mockDeckRepository.Object, _mockLogger.Object);
        _output = output;
    }

    [Fact]
    public async Task TestGetAllNoDecksFound()
    {
        // arrange
        _mockDeckRepository.Setup(repo => repo.GetAll()).ReturnsAsync((IEnumerable<Deck>?)null!);

        // act
        var result = await _deckController.GetAll();

        //assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Deck list not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetAllOk()
    {
        // arrange
        var creationDate = DateTime.Now;
        var deckList = new List<Deck>()
        {
            new Deck
            {
                DeckId = 1,
                DeckName = "TestDeck1",
                DeckDescription = "Dummy deck1",
                CreationDate = creationDate
            },
            new Deck
            {
                DeckId = 2,
                DeckName = "TestDeck2",
                DeckDescription = "Dummy deck2",
                CreationDate = creationDate
            },
            new Deck
            {
                DeckId = 3,
                DeckName = "TestDeck3",
                DeckDescription = "Dummy deck3",
                CreationDate = creationDate
            }
        };
        _mockDeckRepository.Setup(repo => repo.GetAll()).ReturnsAsync(deckList);

        //act
        var result = await _deckController.GetAll();

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultDecks = Assert.IsType<List<Deck>>(okResult.Value);
        Assert.Equal(deckList.Count, resultDecks.Count);

        // each attribute of the test deckList and the one returned from the controller are compared to each other
        for (int i = 0; i < deckList.Count; i++)
        {
            Assert.Equal(deckList[i].DeckId, resultDecks[i].DeckId);
            Assert.Equal(deckList[i].DeckName, resultDecks[i].DeckName);
            Assert.Equal(deckList[i].DeckDescription, resultDecks[i].DeckDescription);
            Assert.Equal(deckList[i].CreationDate, resultDecks[i].CreationDate);
        }
    }

    [Fact]
    public async Task TestGetDecksByFolderIdDecksNotFound()
    {
        //arrange
        int testFolderId = 1;
        _mockDeckRepository.Setup(repo => repo.GetDecksByFolderId(testFolderId)).ReturnsAsync((IEnumerable<Deck>?)null!);

        // act
        var result = await _deckController.GetDecksByFolderId(testFolderId);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Deck list not found", badRequestResult.Value);
    }

    [Fact]
    public async Task TestGetDecksByFolderIdOk()
    {
        // arrange
        var creationDate = DateTime.Now;
        var testFolderId = 1;
        var deckList = new List<Deck>()
        {
            new Deck
            {
                DeckId = 1,
                DeckName = "TestDeck1",
                DeckDescription = "Dummy deck1",
                CreationDate = creationDate,
                FolderId = testFolderId
            },
            new Deck
            {
                DeckId = 2,
                DeckName = "TestDeck2",
                DeckDescription = "Dummy deck2",
                CreationDate = creationDate,
                FolderId = testFolderId

            },
            new Deck
            {
                DeckId = 3,
                DeckName = "TestDeck3",
                DeckDescription = "Dummy deck3",
                CreationDate = creationDate,
                FolderId = testFolderId
           },
            new Deck
            {
                DeckId = 4,
                DeckName = "TestDeck4",
                DeckDescription = "Dummy deck4",
                CreationDate = creationDate,
                FolderId = testFolderId
            }
        };
        _mockDeckRepository.Setup(repo => repo.GetDecksByFolderId(testFolderId)).ReturnsAsync(deckList);

        // act
        var result = await _deckController.GetDecksByFolderId(testFolderId);
        Assert.NotNull(result);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultDecks = Assert.IsType<List<Deck>>(okResult.Value);
        Assert.Equal(4, resultDecks.Count);

        // id starts with 0, but the list indexing start with 0
        for (int i = 0; i< resultDecks.Count; i++) 
        {
            Assert.IsType<int>(resultDecks[i].DeckId);
            Assert.Equal(i+1, resultDecks[i].DeckId);

            Assert.IsType<string>(resultDecks[i].DeckName);
            Assert.Equal("TestDeck"+(i+1), resultDecks[i].DeckName);

            Assert.IsType<string>(resultDecks[i].DeckDescription);
            Assert.Equal("Dummy deck"+(i+1), resultDecks[i].DeckDescription);

            Assert.IsType<int>(resultDecks[i].FolderId);
            Assert.Equal(testFolderId, resultDecks[i].FolderId);
        }
    }
    [Fact]
    public async Task TestCreateInvalidDeck()
    {
        // act 
        var result = await _deckController.Create(null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid Deck data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestCreateOk()
    {
        // arrange
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        _mockDeckRepository.Setup(repo => repo.Create(testDeck)).ReturnsAsync(true);

        // act
        var result = await _deckController.Create(testDeck);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Deck created successfully", responseMessage);
    }

    [Fact]
    public async Task TestCreateNotOk()
    {
        //arrange
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        _mockDeckRepository.Setup(repo => repo.Create(testDeck)).ReturnsAsync(false);

        //act
        var result = await _deckController.Create(testDeck);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Deck creation failed", responseMessage);
    }
    [Fact]
    public async Task TestCreateInFolderInvalidDeck()
    {
        // act 
        var result = await _deckController.CreateInFolder(It.IsAny<int>(), null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid Deck data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestCreateInFolderOk()
    {
        // arrange
        var testFolderId = 1;
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        var testDeckWithFolderId = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now,
            FolderId = testFolderId
        };

        _mockDeckRepository.Setup(repo => repo.Create(It.IsAny<Deck>())).ReturnsAsync(true);

        // act
        var result = await _deckController.CreateInFolder(testFolderId, testDeck);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Deck created successfully", responseMessage);
    }

    [Fact]
    public async Task TestCreateInFolderNotOk()
    {
        // arrange
        var testFolderId = 1;
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        var testDeckWithFolderId = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now,
            FolderId = testFolderId
        };

        _mockDeckRepository.Setup(repo => repo.Create(testDeck)).ReturnsAsync(false);

        //act
        var result = await _deckController.CreateInFolder(testFolderId, testDeck);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Deck creation failed", responseMessage);
    }

    [Fact]
    public async Task TestGetDeckByIdInvalidDeck()
    {
        // arrange
        int testId = 1;
        _mockDeckRepository.Setup(repo => repo.GetDeckById(testId)).ReturnsAsync((Deck?)null);

        // act
        var result = await _deckController.GetDeckbyId(testId);

        // assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Deck not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetDeckByIdOk()
    {
        // arrange
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        int testId = 1;

        _mockDeckRepository.Setup(repo => repo.GetDeckById(testId)).ReturnsAsync(testDeck);

        // act
        var result = await _deckController.GetDeckbyId(testId);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDeck = Assert.IsType<Deck>(okResult.Value);
        Assert.Equal(testDeck, returnedDeck);
    }

    [Fact]
    public async Task TestUpdateInvalidDeck()
    {
        //arrange
        _mockDeckRepository.Setup(repo => repo.Update(null!)).ReturnsAsync(false);

        //act
        var result = await _deckController.Update(null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid Deck data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestUpdateOk()
    {
        //arrange
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        _mockDeckRepository.Setup(repo => repo.Update(testDeck)).ReturnsAsync(true);

        //act
        var result = await _deckController.Update(testDeck);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Deck #" + testDeck.DeckId + "updated successfully", responseMessage);
    }

    [Fact]
    public async Task TestUpdateNotOk()
    {
        // Arrange
        var testDeck = new Deck
        {
            DeckId = 1,
            DeckName = "TestDeck1",
            DeckDescription = "Dummy deck1",
            CreationDate = DateTime.Now
        };

        _mockDeckRepository.Setup(repo => repo.Update(testDeck)).ReturnsAsync(false);

        // Act
        var result = await _deckController.Update(testDeck);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Deck update failed", responseMessage);
    }

    [Fact]
    public async Task TestDeleteDeckNotOk()
    {
        //arrange
        var testId = 1;
        _mockDeckRepository.Setup(repo => repo.Delete(testId)).ReturnsAsync(false);

        //act
        var result = await _deckController.DeleteDeck(testId);

        //assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var badRequestMessage = Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Deck deletion failed", badRequestMessage);
    }

    [Fact]
    public async Task TestDeleteDeckOk()
    {
        //arrange
        var testId = 1;
        _mockDeckRepository.Setup(repo => repo.Delete(testId)).ReturnsAsync(true);

        //act
        var result = await _deckController.DeleteDeck(testId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Deck #" + testId.ToString() + " deleted successfully", responseMessage);
    }
}
