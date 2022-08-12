namespace ShiftWatcher.Models;

public record Orcicorn(ActiveCodeGroups[] ActiveCodeGroups);
public record ActiveCodeGroups(Meta Meta, CodeInfo[] Codes);
public record Meta(string Version, string Description, string Attribution, string Permalink, Generated Generated);
public record Generated(string Epoch, string Human);
public record CodeInfo(string Code, string Type, string Game, string Platform, string Reward, string Archived, string Expires, string Link);