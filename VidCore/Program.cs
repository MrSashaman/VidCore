using VidCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

Database.Initialize();
CommentDatabase.Initialize();
MusicDatabase.Initialize();

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Error404");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
