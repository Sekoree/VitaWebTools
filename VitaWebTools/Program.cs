using BlazorDownloadFile;
using MudBlazor;
using MudBlazor.Services;
using VitaWebTools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var textDictionary = new Dictionary<string, string>();
var textDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Pages/PageTexts/EN"));
foreach (var textFile in textDirectory.GetFiles())
{ 
    textDictionary.Add(textFile.Name.Replace(".md", string.Empty), await File.ReadAllTextAsync(textFile.FullName));   
}

//Idk i think i just need 1
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<Utilities>();
builder.Services.AddSingleton<HomebrewUtility>();
builder.Services.AddSingleton(textDictionary);

builder.Services.AddMudServices(); 
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazorDownloadFile();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
