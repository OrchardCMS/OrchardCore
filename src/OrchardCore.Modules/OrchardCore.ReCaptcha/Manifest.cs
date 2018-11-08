using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ReCaptcha",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0")]

[assembly: Feature(
    Id = "OrchardCore.ReCaptcha",
    Name = "ReCaptcha",
    Category = "Security",
    Description = "Provides ReCaptcha functionality.")]

[assembly: Feature(
    Id ="OrchardCore.ReCaptcha.User.Login",
    Name = "ReCaptcha User Login",
    Description = "Activates a captcha when the user attempts to login too many times.",
    Category = "Security",
    Dependencies = new []{ "OrchardCore.ReCaptcha", "OrchardCore.Users" })]

[assembly: Feature(
    Id = "OrchardCore.ReCaptcha.User.RegisterAccount",
    Name = "ReCaptcha Register New Account",
    Description = "Activates a captcha when a user creates a new account.",
    Category = "Security",
    Dependencies = new[] { "OrchardCore.ReCaptcha", "OrchardCore.Users.Registration" })]

[assembly: Feature(
    Id = "OrchardCore.ReCaptcha.User.ForgotPassword",
    Name = "ReCaptcha Forgot Password",
    Description = "Activates a captcha when a user tries to reset his password.",
    Category = "Security",
    Dependencies = new[] { "OrchardCore.ReCaptcha", "OrchardCore.Users.ResetPassword" })]
