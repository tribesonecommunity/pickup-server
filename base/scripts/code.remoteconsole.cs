$RemoteConsole::LineCount = 4;
$RemoteConsole::MinAccessRequired = GetAdminLevel("God");

Attachment::AddAfter("Console::EchoPipe", "RemoteConsole::EchoPipe");
Attachment::AddAfter("displayMenuServerToggles", "RemoteConsole::displayMenuServerToggles");
Attachment::AddAfter("processMenuServerToggleMenu", "RemoteConsole::processMenuServerToggleMenu");
Attachment::AddBefore("remoteSay", "RemoteConsole::remoteSay");

function Console::EchoPipe(%text){}

function RemoteConsole::EchoPipe(%text)
{
  %idx = -1;
  while($RemoteConsole::Text[%idx++]!="") {}

  if(%idx >= $RemoteConsole::LineCount)
  {
    for(%i = 0; %i <= $RemoteConsole::LineCount; %i++)
      $RemoteConsole::Text[%i] = $RemoteConsole::Text[%i+1];
    %idx = $RemoteConsole::LineCount;
  }
  $RemoteConsole::Text[%idx] = escapeString(%text);

  // update clients
  for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
  {
    if(%cl.remoteConsole)
      RemoteConsole::Update(%cl);
   }
}

function RemoteConsole::Update(%clientId)
{
  %line = 0;
  if(%clientId.remoteConsole == true)
  {
    for(%idx = $RemoteConsole::LineCount; %idx >= 0; %idx--)
      %text = $RemoteConsole::Text[%idx] @ "\n" @ %text;
    BottomPrint(%clientId, %text, 9999);
  }
  else
    BottomPrint(%clientId, "");
}

function RemoteConsole::remoteSay(%clientId, %team, %message)
{
  if(%clientId.remoteConsole)
  {
    eval(%message);
    return("halt");
  }
}