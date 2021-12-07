function zadmin::ZeroPad(%s)
{
	if (String::Len(%s)<2)
		%s = "0" @ %s;
	return %s;
}

function zadmin::getTimeStamp()
{
	if ($zadmin::pref::timestamper == "TWL")
		return zadmin::getTimeStamp_TWL();
	else if ($zadmin::pref::timestamper == "Timepoet")
		return zadmin::getTimeStamp_Timepoet();
	else if ($zadmin::pref::timestamper == "Patched")
		return zadmin::GetTimeStamp_Patched();
}

function zadmin::getFileTimeStamp()
{
	if ($zadmin::pref::timestamper == "Patched")
	{
		Time::Array();
		%suffix = "." @ $Time[yr] @ "-" @ $Time[mo] @ "-" @ $Time[dy];
	}
	else if ($zadmin::pref::timestamp == "TWL")
	{
		zadmin::getTimeStamp();
		%suffix = "." @ $TWL::Year @ "-" @ $TWL::Month @ "-" @ $TWL::Day;
	}
	else if ($zadmin::pref::timestamp == "Timepoet")
	{
		zadmin::getTimeStamp();

		%date = $TPDate;
		%day = getWord(%date, 0);
		%month = getWord(%date, 1);
		%year = getWord(%date, 2);
		%suffix = "." @ %year @ "-" @ %month @ "-" @ %day;
	}
	else
	{
		%suffix = "";
	}
	
	//this is freaking gay, its better to have a script rotate logs for you
	return "";
}

function zadmin::getTimeStamp_TWL()
{
	if( isFile("config\\TWLTimeLog.cs") )
		exec("TWLTimeLog.cs");
	else
		return "";

	// whee...
	%hours = $TWL::Hour;
	%minutes = $TWL::Min;

	%hours += $zadmin::pref::server::TimeOffset;

	%daysinc = 0;

	if( %hours >= 24 )
	{
		%hours -= 24;
		%daysinc = 1;
	}

	%logtime = zadmin::ZeroPad(%hours) @ ":" @ zadmin::ZeroPad(%minutes);

	%date = $TPDate;

	%day = $TWL::Day;
	%month = $TWL::Month;
	%year = $TWL::Year;

	if( %daysinc )
	{
		%day++;
	
		if( endMonth(%month, %day, %year) )
		{
			%month++;
			%day = "01";
		}
	
		if( %month > 12 )
		{
			%month -= 12;
			%year++;
		}
	}

	%logdate = %year @ "-" @ zadmin::ZeroPad(%month) @ "-" @ zadmin::ZeroPad(%day);

	return %logdate @ " " @ %logtime;   
}

function zadmin::getTimeStamp_Timepoet()
{
	if( isFile("config\\Timepoet.cs") )
		exec("Timepoet.cs");
	else
		return "";

   // whee...
   %time = $TPTime;
   %hours = string::getSubStr( %time, 0, 2 );
   %minutes = string::getSubStr( %time, 3, 2 );
   
   %hours += $zAdmin::ServerTimeOffset;
   
   %daysinc = 0;

   
   if( %hours >= 24 )
   {
      %hours -= 24;
      %daysinc = 1;
   }
   
  
   %logtime = zadmin::ZeroPad(%hours) @ ":" @ zadmin::ZeroPad(%minutes);
   
   %date = $TPDate;
   
   %day = getWord(%date, 0);
   %month = getWord(%date, 1);
   %year = getWord(%date, 2);
   
   if( %daysinc )
   {
      %day++;
      if( endMonth(%month, %day, %year) )
      {
	 	 %day = "01";
         %month++;
      }
      if( %month > 12 )
      {
         %month -= 12;
         %year++;
      }
   }
   
   %logdate = %year @ "-" @ zadmin::ZeroPad(%month) @ "-" @ zadmin::ZeroPad(%day);
   
   return %logdate @ " " @ %logtime;   
}


function zadmin::getTimeStamp_Patched()
{
	Time::Array();
	
	// whee...
	%hours = $Time[hr];
	%minutes = $Time[mn];

	%hours += $zadmin::pref::server::TimeOffset;

	%daysinc = 0;
	if( %hours >= 24 )
	{
		%hours -= 24;
		%daysinc = 1;
	}

	%logtime = zadmin::ZeroPad(%hours) @ ":" @ zadmin::ZeroPad(%minutes);

	%day = $Time[dy];
	%month = $Time[mo];
	%year = $Time[yr];

	if( %daysinc )
	{
		%day++;
	
		if( endMonth(%month, %day, %year) )
		{
			%month++;
			%day = "01";
		}
	
		if( %month > 12 )
		{
			%month -= 12;
			%year++;
		}
	}

	%logdate = %year @ "-" @ zadmin::ZeroPad(%month) @ "-" @ zadmin::ZeroPad(%day);

	return %logdate @ " " @ %logtime;   
}


function Time::Array()
{
	%str = timestamp();

	$Time[yr] = String::GetSubStr(%str, 00, 04);
	$Time[mo] = String::GetSubStr(%str, 05, 02);
	$Time[dy] = String::GetSubStr(%str, 08, 02);
	$Time[hr] = String::GetSubStr(%str, 11, 02);
	$Time[mn] = String::GetSubStr(%str, 14, 02);
	$Time[sc] = String::GetSubStr(%str, 17, 02);
	$Time[ms] = String::GetSubStr(%str, 20, 03);
}


function endMonth(%month, %day, %year)
{
	if( %month == "01" && %day > "31" ) return 1;
	if( %month == "02" && ( (((%year - 1998) % 4 == 0) && %day > "29") || (((%year - 1998) % 4 != 0) && %day > "28") ) ) return 1; // leap year sux
	if( %month == "03" && %day > "31" ) return 1;
	if( %month == "04" && %day > "30" ) return 1;
	if( %month == "05" && %day > "31" ) return 1;
	if( %month == "06" && %day > "30" ) return 1;
	if( %month == "07" && %day > "31" ) return 1;
	if( %month == "08" && %day > "31" ) return 1;
	if( %month == "09" && %day > "30" ) return 1;
	if( %month == "10" && %day > "31" ) return 1;
	if( %month == "11" && %day > "30" ) return 1;
	if( %month == "12" && %day > "31" ) return 1;
	else
		return 0;
}   
