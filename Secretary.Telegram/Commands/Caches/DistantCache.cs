﻿using Newtonsoft.Json;
using Secretary.Documents.Creators;
using Secretary.Documents.Creators.Data;
using Secretary.Documents.Creators.DocumentCreators;
using Secretary.Documents.Creators.MessageCreators;
using Secretary.Storage.Models;
using Secretary.Telegram.Commands.Distant;
using Secretary.Telegram.Commands.TimeOff;
using Secretary.Telegram.Models;

namespace Secretary.Telegram.Commands.Caches;

public class DistantCache: 
    IEquatable<DistantCache>, 
    IDistantDocumentCache
{
    [JsonIgnore] public virtual string DocumentKey => DistantCommand.Key;
    public virtual DatePeriod? Period { get; set; }
    public string? Reason { get; set; }
    
    public IEnumerable<Email>? Emails { get; set; }

    public virtual string? FilePath { get; set; }

    public virtual string CreateDocument(User user)
    {
        var data = new DistantData()
        {
            Period = Period!.RawValue,
            Reason = Reason!,
            Name = user.NameGenitive!,
            JobTitle = user.JobTitleGenitive!
        };

        var document = new DistantDocumentCreator().Create(data);

        return document;
    }

    public virtual string CreateMail(User user)
    {
        var data = new DistantData()
        {
            Period = Period!.RawValue,
            Reason = Reason!,
            Name = user.Name!,
            JobTitle = user.JobTitle!
        };

        var document = new DistantMessageCreator().Create(data);

        return document;
    }

    public bool Equals(DistantCache? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Emails == null && other.Emails != null) return false;
        if (Emails != null && other.Emails == null) return false;
        if (Period == null && other.Period != null) return false;
        if (Period != null && other.Period == null) return false;
        
        return (Period == other.Period || Period.Equals(other.Period)) 
               && Reason == other.Reason 
               && (Emails == other.Emails || Emails!.SequenceEqual(other.Emails!))
               && FilePath == other.FilePath;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DistantCache)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Period, Reason, Emails, FilePath);
    }
}