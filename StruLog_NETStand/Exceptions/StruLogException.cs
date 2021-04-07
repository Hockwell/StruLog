using System;

namespace StruLog.Exceptions
{
    public class StruLogException : Exception
    {
        public StruLogException(string message) : base(message)
        {

        }
    }
    public class StruLogConfigException : StruLogException
    {
        public StruLogConfigException(string message) : base(message)
        {

        }
    }

    public class StruLogMongoException : StruLogException
    {
        public StruLogMongoException(string message) : base(message)
        {

        }
    }

    public class StruLogFileException : StruLogException
    {
        public StruLogFileException(string message) : base(message)
        {

        }
    }
    public class StruLogConsoleException : StruLogException
    {
        public StruLogConsoleException(string message) : base(message)
        {

        }
    }
}
