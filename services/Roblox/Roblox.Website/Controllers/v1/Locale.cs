using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/locale/v1")]
public class LocaleControllerV1
{
    [HttpGet("locales")]
    public dynamic GetLocales()
    {
        return new
        {
            data = new List<dynamic>()
            {
                new
                {
                    locale = new
                    {
                        id = 1,
                        locale = "en_us",
                        name = "English(US)",
                        nativeName = "English",
                        language = new
                        {
                            id = 41,
                            name = "English",
                            nativeName = "English",
                            languageCode = "en"
                        }
                    },
                    isEnabledForFullExperience = true,
                    isEnabledForSignupAndLogin = true,
                    isEnabledForInGameUgc = true,
                }
            },
        };
    }

    [HttpGet("locales/user-localization-locus-supported-locales")]
    public dynamic GetLocusSupportedLocales()
    {
        return new Dictionary<string, dynamic>()
        {
            {
                "signupAndLogin", new
                {
                    id = 1,
                    locale = "en_us",
                    name = "English(US)",
                    nativeName = "English",
                    language = new
                    {
                        id = 41,
                        name = "English",
                        nativeName = "English",
                        languageCode = "en",
                    }
                }
            },
            {
                "generalExperience", new
                {
                    id = 1,
                    locale = "en_us",
                    name = "English(US)",
                    nativeName = "English",
                    language = new
                    {
                        id = 41,
                        name = "English",
                        nativeName = "English",
                        languageCode = "en",
                    }
                }
            },
            {
                "ugc", new
                {
                    id = 1,
                    locale = "en_us",
                    name = "English(US)",
                    nativeName = "English",
                    language = new
                    {
                        id = 41,
                        name = "English",
                        nativeName = "English",
                        languageCode = "en",
                    }
                }
            }
        };
    }
}