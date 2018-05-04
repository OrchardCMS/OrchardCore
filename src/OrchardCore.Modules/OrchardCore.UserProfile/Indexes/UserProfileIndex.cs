using YesSql.Indexes;

namespace OrchardCore.UserProfile.Indexes
{
    public class UserProfileIndex : MapIndex
    {
        public string TimeZone { get; set; }
    }

    public class UserProfileIndexProvider : IndexProvider<Models.UserProfile>
    {
        public override void Describe(DescribeContext<Models.UserProfile> context)
        {
            context.For<UserProfileIndex>()
                .Map(userProfile =>
                {
                    return new UserProfileIndex
                    {
                        TimeZone = userProfile.TimeZone
                    };
                });
        }
    }
}