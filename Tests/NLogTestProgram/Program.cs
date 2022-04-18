// See https://aka.ms/new-console-template for more information
using NLog;

var collapsers = Bliptech.Blipboard.NLog.CollapsePatternRegistry.Default;

collapsers.Add("secrets", @"\\(secret.*\\)");



var log = LogManager.GetLogger("Simple");

log.Info("Hello, World!");

// The raw message template is transmitted separately
// from the combined message
log.Info("This is a {adj} day.", "good");

// Numbers, brackets and other things can be collapsed
// on the client side with a fixed set of regexes
log.Info("The number 42 answers 'it' all.");

// Exception stack traces appear in the details
try
{
    throw new Exception("A test exception");
}
catch (Exception ex)
{
    log.Info(ex, "This test exception was caught");
}

log.Info("A message with no secrets (secret is foo)");



var log2 = LogManager.GetLogger("Modified");

try
{
    throw new Exception("A test exception");
}
catch (Exception ex)
{
    log2.Info(ex, "This test exception was also caught");
}
