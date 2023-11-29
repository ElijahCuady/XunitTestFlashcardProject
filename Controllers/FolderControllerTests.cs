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
public class FolderControllerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<FolderController>> _mockLogger;
    private readonly FolderController _folderController;
    private readonly ITestOutputHelper _output;
    public FolderControllerTests(ITestOutputHelper output)
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<FolderController>>();
        _folderController = new FolderController(_mockFolderRepository.Object, _mockLogger.Object);
        _output = output;
    }

    [Fact]
    public async Task TestGetAllNoFoldersFound()
    {
        // arrange
        _mockFolderRepository.Setup(repo => repo.GetAll()).ReturnsAsync((IEnumerable<Folder>?) null!);

        // act
        var result = await _folderController.GetAll();

        //assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Folder list not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetAllOk()
    {
        // arrange
        var creationDate =DateTime.Now;
        var folderList = new List<Folder>()
        {
            new Folder
            {
                FolderId = 1,
                FolderName = "TestFolder1",
                FolderDescription = "Dummy folder1",
                CreationDate = creationDate
            },
            new Folder
            {
                FolderId = 2,
                FolderName = "TestFolder2",
                FolderDescription = "Dummy folder2",
                CreationDate = creationDate
            },
            new Folder
            {
                FolderId = 3,
                FolderName = "TestFolder3",
                FolderDescription = "Dummy folder3",
                CreationDate = creationDate
            }
        };
        _mockFolderRepository.Setup(repo => repo.GetAll()).ReturnsAsync(folderList);

        //act
        var result = await _folderController.GetAll();

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultFolders = Assert.IsType<List<Folder>>(okResult.Value);
        Assert.Equal(folderList.Count, resultFolders.Count);

        // each attribute of the test folderList and the one returned from the controller are compared to each other
        for (int i = 0; i < folderList.Count; i++)
        {
            Assert.Equal(folderList[i].FolderId, resultFolders[i].FolderId);
            Assert.Equal(folderList[i].FolderName, resultFolders[i].FolderName);
            Assert.Equal(folderList[i].FolderDescription, resultFolders[i].FolderDescription);
            Assert.Equal(folderList[i].CreationDate, resultFolders[i].CreationDate);
        }
    }

    [Fact]
    public async Task TestCreateInvalidFolder()
    {
        // act 
        var result = await _folderController.Create(null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid folder data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestCreateOk()
    {
        // arrange
        var testFolder = new Folder
        {
            FolderId = 1,
            FolderName = "TestFolder1",
            FolderDescription = "Dummy folder1",
            CreationDate = DateTime.Now
        };

        _mockFolderRepository.Setup(repo => repo.Create(testFolder)).ReturnsAsync(true);

        // act
        var result = await _folderController.Create(testFolder);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Folder created successfully", responseMessage);
    }

    [Fact]
    public async Task TestCreateNotOk()
    {
        //arrange
        var testFolder = new Folder
        {
            FolderId = 1,
            FolderName = "TestFolder1",
            FolderDescription = "Dummy folder1",
            CreationDate = DateTime.Now
        };

        _mockFolderRepository.Setup(repo => repo.Create(testFolder)).ReturnsAsync(false);

        //act
        var result = await _folderController.Create(testFolder);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Folder creation failed", responseMessage);
    }

    [Fact]
    public async Task TestGetFolderByIdInvalidFolder()
    {
        // arrange
        int testId = 1;
        _mockFolderRepository.Setup(repo => repo.GetFolderById(testId)).ReturnsAsync((Folder?)null);

        // act
        var result = await _folderController.GetFolderbyId(testId);

        // assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.IsType<string>(notFoundResult.Value);
        Assert.Equal("Folder not found", notFoundResult.Value);
    }

    [Fact]
    public async Task TestGetFolderByIdOk()
    {
        // arrange
        var testFolder = new Folder
        {
            FolderId = 1,
            FolderName = "TestFolder1",
            FolderDescription = "Dummy folder1",
            CreationDate = DateTime.Now
        };

        int testId = 1;

        _mockFolderRepository.Setup(repo => repo.GetFolderById(testId)).ReturnsAsync(testFolder);

        // act
        var result = await _folderController.GetFolderbyId(testId);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFolder = Assert.IsType<Folder>(okResult.Value);
        Assert.Equal(testFolder, returnedFolder);
    }

    [Fact]
    public async Task TestUpdateInvalidFolder()
    {
        //arrange
        _mockFolderRepository.Setup(repo => repo.Update(null!)).ReturnsAsync(false);

        //act
        var result = await _folderController.Update(null!);

        // assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid folder data", badRequestResult.Value);
    }

    [Fact]
    public async Task TestUpdateOk()
    {
        //arrange
        var testFolder = new Folder
        {
            FolderId = 1,
            FolderName = "TestFolder1",
            FolderDescription = "Dummy folder1",
            CreationDate = DateTime.Now
        };

        _mockFolderRepository.Setup(repo => repo.Update(testFolder)).ReturnsAsync(true);

        //act
        var result = await _folderController.Update(testFolder);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Folder #" + testFolder.FolderId + "updated successfully", responseMessage);
    }

    [Fact]
    public async Task TestUpdateNotOk()
    {
        // Arrange
        var testFolder = new Folder
        {
            FolderId = 1,
            FolderName = "TestFolder1",
            FolderDescription = "Dummy folder1",
            CreationDate = DateTime.Now
        };

        _mockFolderRepository.Setup(repo => repo.Update(testFolder)).ReturnsAsync(false);

        // Act
        var result = await _folderController.Update(testFolder);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.False(responseTrue);
        Assert.Equal("Folder update failed", responseMessage);
    }

    [Fact]
    public async Task TestDeleteFolderNotOk()
    {
        //arrange
        var testId = 1;
        _mockFolderRepository.Setup(repo => repo.Delete(testId)).ReturnsAsync(false);

        //act
        var result = await _folderController.DeleteFolder(testId);

        //assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var badRequestMessage = Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Folder deletion failed", badRequestMessage);
    }

    [Fact]
    public async Task TestDeleteFolderOk()
    {
        //arrange
        var testId = 1;
        _mockFolderRepository.Setup(repo => repo.Delete(testId)).ReturnsAsync(true);

        //act
        var result = await _folderController.DeleteFolder(testId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        //getting the properties of the response that the controller method returns 
        var resultProperties = okResult.Value.GetType().GetProperties();
        var responseTrue = Assert.IsType<bool>(resultProperties[0].GetValue(okResult.Value));
        var responseMessage = Assert.IsType<string>(resultProperties[1].GetValue(okResult.Value));

        Assert.True(responseTrue);
        Assert.Equal("Folder #" + testId.ToString() + " deleted successfully", responseMessage);
    }
}
