﻿using secretary.storage.models;
using secretary.telegram.commands.validation;
using secretary.telegram.exceptions;

namespace secretary.telegram.tests.commands.validators;

public class UserValidationVisitorTests
{
    private UserValidationVisitor _visitor = null!;

    [SetUp]
    public void Setup()
    {
        _visitor = new UserValidationVisitor();
    }

    [Test]
    public void ShouldReturnExceptionForNullReference()
    {
        Assert.Throws<NonCompleteUserException>(() => _visitor.Validate(null));
    }

    [Test]
    public void ShouldReturnExceptionForUserWithoutToken()
    {
        Assert.Throws<NonCompleteUserException>(() => _visitor.Validate(new User() { JobTitleGenitive = ""}));
    }

    [Test]
    public void ShouldReturnExceptionForUserInfo()
    {
        Assert.Throws<NonCompleteUserException>(() => _visitor.Validate(new User() { AccessToken = ""}));
    }

    [Test]
    public void ShouldPassFullUser()
    {
        _visitor.Validate(new User()
        {
            ChatId = 2517,
            Name = "Name",
            NameGenitive = "NameGenitive",
            JobTitle = "JobTitle",
            JobTitleGenitive = "JobTitleGenitive",
            Email = "Email",
            AccessToken = "AccessToken",
            RefreshToken = "RefreshToken",
        });
    }
}