using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var students = new List<Student>() {

    new Student(){

        Name="张三",
        Age=16
    }
};

app.MapGet("/Student", () =>
{
    return students;
});

app.MapPost("/Student", () =>
{
    students.Add(new Student()
    {
        Name = "Name" + Guid.NewGuid()
    });

    return "添加成功";
});


app.Run();

