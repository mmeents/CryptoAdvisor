using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C0DEC0RE {

  public class MMSQLStrings {
    //  My favorite SQL functions 

    public string Obj_fnDBObjectExists(){ 
      return 
        "-- fnDBObjectExists, Author Matt Meents checks to see if the database object exists. "+Environment.NewLine+
        "Create function [dbo].[fnDBObjectExists] ( @aObjectName varchar(150) )  returns bit as "+Environment.NewLine+
        "begin "+Environment.NewLine+ 
        "  declare @a bit set @a = ( select case when exists( "+Environment.NewLine+
        "    select so.xtype ObjType, so.name tbl, sc.name col, st.name ColType, sc.length ColLen "+Environment.NewLine+ 
        "      from dbo.sysobjects so "+Environment.NewLine+
        "        left outer join dbo.syscolumns sc on so.id=sc.id "+Environment.NewLine+
        "        left outer join ( "+Environment.NewLine+
        "          select Name, min(UserType) UserType, xtype from dbo.systypes Group by Name, xtype  "+Environment.NewLine+
        "        ) st on sc.UserType=st.UserType and sc.xtype=st.xtype  "+Environment.NewLine+
        "      where so.xtype  in ('U','V','P','FN','TR')  "+Environment.NewLine+
        "        and (so.Name not like ('dt_%')) and (so.Name not like ('sys%')) "+Environment.NewLine+ 
        "        and (so.Name = @aObjectName) "+Environment.NewLine+
        "  ) then 1 else 0 end )"+Environment.NewLine+
        "  return (@a)  "+Environment.NewLine+
        "end ";
    }

    public string Obj_fnParseDate(){ 
      return 
        "ALTER function [dbo].[fnParseDate](@inputVar varchar(1000)) returns datetime as "+Environment.NewLine+
        "begin "+Environment.NewLine+
        "  declare @aResult datetime "+Environment.NewLine+
        "  if ( isnull(@inputVar, '' ) != '' ) begin "+Environment.NewLine+
        "   set @aResult = TRY_PARSE( @inputVar as datetime) "+Environment.NewLine+
        "  end "+Environment.NewLine+
        "  return @aResult "+Environment.NewLine+
        "end";
    }

    public string Obj_fnParseCount(){ 
      return 
        "-- fnParseCount, Author Matt Meents breaks a varchar by characters within delimiter var, returns count. "+Environment.NewLine+
        "Create Function [dbo].[fnParseCount](@s varchar(4000), @delimiter varchar(100)) returns int as begin"+Environment.NewLine+
        "  declare @count int  set @count = 0"+Environment.NewLine+
        "  declare @i int      set @i = 1"+Environment.NewLine+
        "  declare @toggle int set @toggle=1  "+Environment.NewLine+
        "  declare @loop int   set @loop = 0  "+Environment.NewLine+
        "  declare @char varchar(1)  "+Environment.NewLine+
        "  if (len(@s) > 0 ) begin"+Environment.NewLine+
        "    while ( (@i<=len(@s)) and (patindex('%'+substring(@s, @i, 1)+'%', @delimiter ) > 0) ) Begin"+Environment.NewLine+
        "      set @i = @i + 1"+Environment.NewLine+
        "    end      "+Environment.NewLine+
        "    while (@i<=len(@s)) begin"+Environment.NewLine+
        "      If (patindex('%'+substring(@s, @i, 1)+'%', @delimiter ) > 0) begin"+Environment.NewLine+
        "        set @toggle=1"+Environment.NewLine+
        "      end else begin"+Environment.NewLine+
        "        if @toggle=1 begin"+Environment.NewLine+
        "          set @count = @Count + 1"+Environment.NewLine+
        "        end"+Environment.NewLine+
        "        set @toggle = 0"+Environment.NewLine+
        "      end         "+Environment.NewLine+
        "      set @i = @i + 1 "+Environment.NewLine+
        "    end      "+Environment.NewLine+
        "  end else"+Environment.NewLine+
        "    set @Count = 0"+Environment.NewLine+
        "  Return(@Count)"+Environment.NewLine+
        "end";
    }

    public string Obj_fnParseString(){ 
      return 
        "-- fnParseString, Author Matt Meents breaks a varchar by characters within delimiter var, returns value at num string. "+Environment.NewLine+
        "CREATE function [dbo].[fnParseString](@s varchar(4000), @delimiter varchar(100), @num int) returns Varchar(4000) as begin"+Environment.NewLine+
        "  declare @Result varchar(4000)"+Environment.NewLine+
        "  if (dbo.fnParseCount(@s, @delimiter) >= @num ) begin"+Environment.NewLine+
        "    declare @Start int set @Start = 1"+Environment.NewLine+
        "    declare @len int set @len = len(@s)    "+Environment.NewLine+
        "    declare @i int set @i = 1             "+Environment.NewLine+
        "    declare @End int   "+Environment.NewLine+
        "    declare @char varchar(1)        "+Environment.NewLine+
        "    while ( (@Start<=@len) and (patindex('%'+substring(@s, @Start, 1)+'%', @delimiter ) > 0) ) Begin"+Environment.NewLine+
        "      set @Start = @Start + 1"+Environment.NewLine+
        "    end      "+Environment.NewLine+
        "    while (@i<@num ) begin"+Environment.NewLine+
        "      while ( (@Start<=@len) and (patindex('%'+substring(@s, @Start, 1)+'%', @delimiter ) = 0) ) begin"+Environment.NewLine+
        "        set @Start = @start + 1"+Environment.NewLine+
        "      end        "+Environment.NewLine+
        "      while ( (@Start<=@len) and (patindex('%'+substring(@s, @Start, 1)+'%', @delimiter ) > 0) ) begin"+Environment.NewLine+
        "        set @Start = @start + 1"+Environment.NewLine+
        "      end"+Environment.NewLine+
        "      set @i = @i + 1"+Environment.NewLine+
        "    end          "+Environment.NewLine+
        "    set @end = @Start"+Environment.NewLine+
        "    set @i = len(@s)"+Environment.NewLine+
        "    while ( (@end<=@len) and (patindex('%'+substring(@s, @end, 1)+'%', @delimiter ) = 0) ) begin"+Environment.NewLine+
        "      set @end = @end + 1"+Environment.NewLine+
        "    end"+Environment.NewLine+
        "    set @Result = substring(@s, @start, @end-@start)"+Environment.NewLine+
        "  end else"+Environment.NewLine+
        "    set @Result = ''"+Environment.NewLine+
        "  Return(@Result)"+Environment.NewLine+
        "end";
      }
    
    public string Obj_fnRangeOfMonths(){ 
      return 
        "Create FUNCTION [dbo].[fnRangeOfMonths] (@aStartMonthOffset Int, @aRangeLengthMonth int)"+
        " RETURNS @Ranges table (RangeName varchar(200), RangeStart datetime, RangeEnd datetime) "+Environment.NewLine+
        "AS begin "+Environment.NewLine+
        "	-- Fill the table variable with the rows for your result set "+Environment.NewLine+
        "  declare @aDateTime datetime set @aDateTime = dateadd(mm, @aStartMonthOffset, getdate()) "+Environment.NewLine+ 
        "  declare @aMonthStart datetime set @aMonthStart = (select convert(datetime, floor(convert(float, @aDateTime)-Day(@aDateTime)+1 ))) "+Environment.NewLine+ 
        "  declare @aMonthEnd datetime "+Environment.NewLine+
        "  declare @aMonthStop datetime set @aMonthStop = dateadd(mm, @aRangeLengthMonth, @aMonthStart) "+Environment.NewLine+ 
        "  while (@aMonthStart < @aMonthStop) begin "+Environment.NewLine+
        "    set @aMonthEnd = (select Dateadd(ms, -12, DateAdd(m, 1, @aMonthStart))) "+Environment.NewLine+
        "    insert into @Ranges (RangeName, RangeStart, RangeEnd) "+Environment.NewLine+ 
        "      select DateName(month, @aMonthStart)+' '+cast(year(@aMonthStart) as varchar(5)), @aMonthStart, @aMonthEnd "+Environment.NewLine+
        "    set @aMonthStart = dateadd(mm, 1, @aMonthStart) "+Environment.NewLine+
        "  end "+Environment.NewLine+        
        "	 RETURN "+Environment.NewLine+
        "END";
    }

    public string Obj_fnExplodeString(){ 
      return 
        "CREATE function [dbo].[fnExplodeString](@s varchar(1000)) "+
        "  returns @Result table (iRow int, StringPart varchar(100) null) as "+Environment.NewLine+
        "begin"+Environment.NewLine+
        "  declare @delims varchar (50) set @delims = ' '"+Environment.NewLine+
        "  declare @list table (iRow int, StringPart varchar(100) null)"+Environment.NewLine+
        "  declare @i int set @i = 1 "+Environment.NewLine+
        "  declare @j int set @j = dbo.fnParseCount (@s, @delims) "+Environment.NewLine+        
        "  while (@i <= @j) begin"+Environment.NewLine+
        "    insert into @list values (@i, dbo.fnParseString(@s, @delims, @i))"+Environment.NewLine+
        "    set @i = @i + 1"+Environment.NewLine+
        "  end  "+Environment.NewLine+
        "  insert into @Result select Distinct iRow, StringPart from @list order by iRow asc     "+Environment.NewLine+
        "  return"+Environment.NewLine+
        "end ";

    }




    
  }


}
