/*
 * Cppcheck - A tool for static C/C++ code analysis
 * Copyright (C) 2007-2016 Cppcheck team.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#include "_cmdlineparser.h"
#include "cppcheck.h"
#include "_cppcheckexecutor.h"
#include "_filelister.h"
#include "path.h"
#include "settings.h"
#include "timer.h"
#include "check.h"
#include "_threadexecutor.h" // Threading model

#include <algorithm>
#include <iostream>
#include <sstream>
#include <fstream>
#include <string>
#include <cstring>
#include <cstdlib> // EXIT_FAILURE

#ifdef HAVE_RULES
// xml is used for rules
#include <tinyxml2.h>
#endif

 char* ExeLocation;


//////////////Mike read CMD/////////////////////

#include <stdio.h>
#include <string.h>

#define OK       0
#define NO_INPUT 1
#define TOO_LONG 2
static int getLine(char *prmpt, char *buff, size_t sz) {
	int ch, extra;

	// Get line with buffer overrun protection.
	if (prmpt != NULL) {
		printf("%s", prmpt);
		fflush(stdout);
	}
	if (fgets(buff, sz, stdin) == NULL)
		return NO_INPUT;

	// If it was too long, there'll be no newline. In that case, we flush
	// to end of line so that excess doesn't affect the next call.
	if (buff[strlen(buff) - 1] != '\n') {
		extra = 0;
		while (((ch = getchar()) != '\n') && (ch != EOF))
			extra = 1;
		return (extra == 1) ? TOO_LONG : OK;
	}

	// Otherwise remove newline and give string back to caller.
	buff[strlen(buff) - 1] = '\0';
	return OK;
}
int nInputResult;
char aInputBuff[1000];
 char* sNull = "\0";
#define Is_Char(_cVal) ( (_cVal >= 'A' && _cVal <= 'Z')  || (_cVal >= 'a' && _cVal <= 'z')  ||  (_cVal >= '0' && _cVal <= '9') || _cVal == '_'  || _cVal == '\"' || _cVal == '\'' || _cVal == '\\' || _cVal == '/' || _cVal == ':'  || _cVal == '.' || _cVal == '-')



void fPrintElement(const char* _sElement){
	UINT32 i = 0;
	while (Is_Char(_sElement[i]) ) {
		printf("%c", _sElement[i] );
		i++;
	}
	printf("\n");
}
bool fIs(const char* _sCmd, const char* _sCompare){
	UINT32 i = 0;
	while (Is_Char(_sCmd[i]) ) {
		if(_sCompare[i] != _sCmd[i] ){
			return false;
		}

		i++;
	}
	return true;
}



/////////////////////////////////////////////





static void AddFilesToList(const std::string& FileList, std::vector<std::string>& PathNames)
{
//printf("\nAddFilesToList: %s", FileList);
    // To keep things initially simple, if the file can't be opened, just be silent and move on.
    std::istream *Files;
    std::ifstream Infile;
    if (FileList.compare("-") == 0) { // read from stdin
        Files = &std::cin;
    } else {
        Infile.open(FileList.c_str());
        Files = &Infile;
    }
    if (Files && *Files) {
        std::string FileName;
        while (std::getline(*Files, FileName)) { // next line
            if (!FileName.empty()) {
                PathNames.push_back(FileName);
            }
        }
    }
}

static void AddInclPathsToList(const std::string& FileList, std::list<std::string>* PathNames)
{
    // To keep things initially simple, if the file can't be opened, just be silent and move on.
    std::ifstream Files(FileList.c_str());
    if (Files) {
        std::string PathName;
        while (std::getline(Files, PathName)) { // next line
            if (!PathName.empty()) {
                PathName = Path::removeQuotationMarks(PathName);
                PathName = Path::fromNativeSeparators(PathName);

                // If path doesn't end with / or \, add it
                if (PathName.back() != '/')
                    PathName += '/';

                PathNames->push_back(PathName);
            }
        }
    }
}

static void AddPathsToSet(const std::string& FileName, std::set<std::string>* set)
{
    std::list<std::string> templist;
    AddInclPathsToList(FileName, &templist);
    set->insert(templist.begin(), templist.end());
}

CmdLineParser::CmdLineParser(Settings *settings)
    : _settings(settings)
    , _showHelp(false)
    , _showVersion(false)
    , _showErrorMessages(false)
    , _exitAfterPrint(false)
{
}

void CmdLineParser::PrintMessage(const std::string &message)
{
    std::cout << message << std::endl;
}

void CmdLineParser::PrintMessage(const char* message)
{
    std::cout << message << std::endl;
}


/*
bool CmdLineParser::fCompare(const char* _sCmd1, const char* _sLine)
{


}
*/

void CmdLineParser::fParseLine(const char* _sLine)
{
	UINT32 i = 0;

	printf(" \nLine: %s \n", _sLine);

	while (_sLine[i] != 0) {
		if (_sLine[i] == '*') {
			if (_sLine[i + 1] != 0 && _sLine[i + 1] != '*') {
				fParseCmd(&_sLine[i + 1]);
				i++;
			}
		}

		i++;
	}
}



void CmdLineParser::fParseCmd(const char* _sCmd)
{


//printf("\n---Cmd:");
//	fPrintElement( _sCmd);


	fParseVal(_sCmd);
}



void CmdLineParser::fParseVal(const char* _sCmd)
{
	printf("\nCmd: %s\n", _sCmd);
	UINT32 i = 0;
	UINT32 _nCount = 0;
	char* _sVal1 = sNull;
	char* _sVal2 = sNull;
	char* _sVal3 = sNull;
	bool _bDelemiter = false;
	UINT32 _nLength1 = 0;
    UINT32 _nLength2 = 0;


	while (_sCmd[i] != 0 && _sCmd[i] != '*') {
		if (    ! Is_Char(_sCmd[i])  ) {
			_bDelemiter = true;

		}else{
			if(_bDelemiter){
				_bDelemiter = false;
				if (_sVal1 == sNull) {
					_sVal1 = (char*)&_sCmd[i];
				}else if (_sVal2 == sNull) {
                    _nLength1 = _nCount-1;
					_sVal2 = (char*)&_sCmd[i];
				}else if (_sVal3 == sNull) {
                      _nLength2 = _nCount-1;
					_sVal3 = (char*)&_sCmd[i];
				}
				 _nCount = 0;
			}
		}
		i++;
        _nCount++;
	}
	if(_nLength1 == 0){
        _nLength1 = _nCount-1;
	}
    if(_nLength2 == 0){
        _nLength2 = _nCount-1;
	}

	fTestCmdVal(_sCmd,_sVal1, _sVal2,_sVal3 ,_nLength1  ,_nLength2 ,_nCount -1 );
}


void  CmdLineParser::fAddInclude(std::string _path){
	std::string path = Path::removeQuotationMarks(_path);
    path = Path::fromNativeSeparators(path);

    // If path doesn't end with / or \, add it
    if (path.back() != '/'){
        path += '/';
	}
    _settings->includePaths.push_back(path);

	//printf("AddInlcude:%s\n ", _path.c_str());-r
}


void  CmdLineParser::fAddSource(std::string _path){

	  _settings->buildDir = Path::fromNativeSeparators(_path);
        if (_settings->buildDir.back() == '/'){
            _settings->buildDir.erase(_settings->buildDir.size() - 1U);
		}

	  std::string path = Path::removeQuotationMarks(_path);
	//   std::string path = _path;
		path = Path::fromNativeSeparators(path);
		_pathnames.push_back(path);
	///////////////////////////;

}
void  CmdLineParser::fAddDefine(std::string _define){
  // No "=", append a "=1"
    if (_define.find('=') == std::string::npos)
        _define += "=1";

    if (!_settings->userDefines.empty())
        _settings->userDefines += ";";
    _settings->userDefines += _define;
}





void  CmdLineParser::fAddEnable(std::string _sEnable){

		const std::string errmsg = _settings->addEnabled(_sEnable);
        if (!errmsg.empty()) {
            PrintMessage(errmsg);
           // return false;
        }
}



void CmdLineParser::fTestCmdVal(const char* _sCmd, const char* _sVal1, const char* _sVal2, const char* _sVal3, UINT32 _nLength1, UINT32 _nLength2, UINT32 _nLength3)
{
   std::string _sV1(_sVal1, _nLength1);
 //   std::string _sV1(_sVal1, 74);


	      if(fIs(_sCmd, "function"  )){
			printf("function!!!!\n");
	}else if(fIs(_sCmd,  "patate"   )){

		printf("PATAQTEEE!!!!\n");
	}if(  fIs(_sCmd,  "GetClassInfo")   || fIs(_sCmd,  "run" )  ||  fIs(_sCmd,  "parse" )   ){

    printf("-----GetClassInfo!!!!!!!!!!!!: %s \n", _sV1.c_str());
		//printf("_nLength1!!!!--%d\n", _nLength1);
//printf("_nLength1!!!!--%d\n", _sV1.length());


			/////"--cppcheck-build-dir=", 21) == 0)

			//_sVal1 = "E:/_Project/Cwc/_Cwc_Demos/Demos/Base_Example/01_HelloWorld/";


fAddEnable("information");

_settings->maxConfigs = 1; //Only process current defines

  _settings->checkConfiguration = true;

_settings->inconclusive = true; //don't block on errors?
_settings->reportProgress = true;
 _settings->verbose = true;
 _settings->checkLibrary = false;
 _settings->experimental = false;
    _exitAfterPrint = false;
  _settings->dump = true;


			fAddDefine("tPlatform_Windows");
			fAddDefine("_WIN32");
			fAddDefine("__GNUC__=4");
			fAddDefine("__clang__=1");
			fAddDefine("__llvm__=1");
			fAddDefine("__x86_64__");


		//	fAddDefine("__llvm__=1");

			///////Include ///
			if(_sVal1 == sNull){
                _sV1 = "E:/_Project/Cwc/_Cwc_Demos/Demos/Base_Example/01_HelloWorld/HelloWorld.cpp";
			}
      fAddSource(_sV1);

			//////////////////////
			fAddInclude("E:/_Project/Cwc/Modules/GZE/");
			fAddInclude("E:/_Project/Cwc/Modules/GZE/SubLib_System/");



			fAddInclude("E:/_Project/Cwc/Modules/Emscripten_x64/emscripten/1.35.0/system/include/");
			fAddInclude("E:/_Project/Cwc/Modules/LibRT_x64/i686-w64-mingw32/include/");
			fAddInclude("E:/_Project/Cwc/Modules/LibRT_x64/clang_inc/");
			fAddInclude("E:/_Project/Cwc/Modules/LibRT_x64/i686-w64-mingw32/include/c++/i686-w64-mingw32/");






		int _nTest = 0;
		cppexecutor->fRun();
		cppexecutor->check_wrapper(_sV1, *cppcheck, _nTest, &ExeLocation);

			printf("\n--- RUN ---\n");
		// if (settings.terminated()) {
		//		return EXIT_SUCCESS;
		//	}
			//cppCheck.settings().exceptionHandling
				//return cppexecutor->check_wrapper(cppCheck, argc, argv);

				//return cppexecutor->check_internal(cppCheck, argc, argv);




	}





	printf("\n---------\n");
		fPrintElement( _sCmd);
		fPrintElement( _sVal1);
		fPrintElement( _sVal2);
	printf("---------\n");




}



bool CmdLineParser::ParseFromArgs(int argc, const char* const argv[])
{
    printf("cwcAst:Rdy\n");
ExeLocation = ( char*)argv[0];
	while (1) {


		Sleep(20);

		nInputResult = getLine("Enter string> ", aInputBuff, sizeof(aInputBuff));
		if (nInputResult == NO_INPUT) {
			// Extra NL since my system doesn't output that on EOF.
			printf("\nNo input\n");
			return 1;
		}

		if (nInputResult == TOO_LONG) {
			printf("Input too long [%s]\n", aInputBuff);
			return 1;
		}
		if (nInputResult == OK) {

			printf("OK [%s]\n", aInputBuff);
			fParseLine(aInputBuff);
		}

	}


	/*
	for (int i = 1; i < argc; i++) {

		if (i + 1 < argc) {
			ParseFromArg(argv[i], argv[i + 1]);
		}else {
			ParseFromArg(argv[i], 0);
		}
	}*/




	_settings->project.ignorePaths(_ignoredPaths);

	/*
	if (_settings->force)
		_settings->maxConfigs = ~0U;

	else if ((def || _settings->preprocessOnly) && !maxconfigs)
		_settings->maxConfigs = 1U;
	*/

	if (_settings->isEnabled("unusedFunction") && _settings->jobs > 1) {
		PrintMessage("cppcheck: unusedFunction check can't be used with '-j' option. Disabling unusedFunction check.");
	}

	if (_settings->xml) {
		// Warn about XML format 1, which will be removed in cppcheck 1.81
		if (_settings->xml_version == 1U)
			PrintMessage("cppcheck: XML format version 1 is deprecated and will be removed in cppcheck 1.81. Use '--xml-version=2'.");
		if (_settings->inconclusive && _settings->xml_version == 1U)
			PrintMessage("cppcheck: inconclusive messages will not be shown, because the old xml format is not compatible.");
	}

	if (argc <= 1) {
		_showHelp = true;
		_exitAfterPrint = true;
	}



	// Print error only if we have "real" command and expect files
	if (!_exitAfterPrint && _pathnames.empty() && _settings->project.fileSettings.empty()) {
		PrintMessage("cppcheck: No C or C++ source files found.");
		return false;
	}

	// Use paths _pathnames if no base paths for relative path output are given
	if (_settings->basePaths.empty() && _settings->relativePaths)
		_settings->basePaths = _pathnames;

}


bool CmdLineParser::ParseFromArg( const char* argv, const char* aNext)
{
    bool def = false;
    bool maxconfigs = false;


        if (argv[0] == '-') {
            if (std::strcmp(argv, "--version") == 0) {
                _showVersion = true;
                _exitAfterPrint = true;
                return true;
            }

            else if (std::strncmp(argv, "--cppcheck-build-dir=", 21) == 0) {
                _settings->buildDir = Path::fromNativeSeparators(argv + 21);
                if (_settings->buildDir.back() == '/')
                    _settings->buildDir.erase(_settings->buildDir.size() - 1U);
            }

            // Flag used for various purposes during debugging
            else if (std::strcmp(argv, "--debug") == 0)
                _settings->debug = _settings->debugwarnings = true;

            // Show --debug output after the first simplifications
            else if (std::strcmp(argv, "--debug-normal") == 0)
                _settings->debugnormal = true;

            // Show debug warnings
            else if (std::strcmp(argv, "--debug-warnings") == 0)
                _settings->debugwarnings = true;

            // dump cppcheck data
            else if (std::strcmp(argv, "--dump") == 0)
                _settings->dump = true;

            // (Experimental) exception handling inside cppcheck client
            else if (std::strcmp(argv, "--exception-handling") == 0)
                _settings->exceptionHandling = true;
            else if (std::strncmp(argv, "--exception-handling=", 21) == 0) {
                _settings->exceptionHandling = true;
                const std::string exceptionOutfilename = &(argv[21]);
                CppCheckExecutor::setExceptionOutput((exceptionOutfilename=="stderr") ? stderr : stdout);
            }

            // Inconclusive checking
            else if (std::strcmp(argv, "--inconclusive") == 0)
                _settings->inconclusive = true;

            // Enforce language (--language=, -x)
            else if (std::strncmp(argv, "--language=", 11) == 0 || std::strcmp(argv, "-x") == 0) {
                std::string str;
                if (argv[2]) {
                    str = argv+11;
                } else {

                    if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: No language given to '-x' option.");
                        return false;
                    }
                    str = aNext;
                }

                if (str == "c")
                    _settings->enforcedLang = Settings::C;
                else if (str == "c++")
                    _settings->enforcedLang = Settings::CPP;
                else {
                    PrintMessage("cppcheck: Unknown language '" + str + "' enforced.");
                    return false;
                }
            }

            // Filter errors
            else if (std::strncmp(argv, "--exitcode-suppressions=", 24) == 0) {
                // exitcode-suppressions=filename.txt
                std::string filename = 24 + argv;

                std::ifstream f(filename.c_str());
                if (!f.is_open()) {
                    PrintMessage("cppcheck: Couldn't open the file: \"" + filename + "\".");
                    return false;
                }
                const std::string errmsg(_settings->nofail.parseFile(f));
                if (!errmsg.empty()) {
                    PrintMessage(errmsg);
                    return false;
                }
            }

            // Filter errors
            else if (std::strncmp(argv, "--suppressions-list=", 20) == 0) {
                std::string filename = argv+20;
                std::ifstream f(filename.c_str());
                if (!f.is_open()) {
                    std::string message("cppcheck: Couldn't open the file: \"");
                    message += filename;
                    message += "\".";
                    if (std::count(filename.begin(), filename.end(), ',') > 0 ||
                        std::count(filename.begin(), filename.end(), '.') > 1) {
                        // If user tried to pass multiple files (we can only guess that)
                        // e.g. like this: --suppressions-list=a.txt,b.txt
                        // print more detailed error message to tell user how he can solve the problem
                        message += "\nIf you want to pass two files, you can do it e.g. like this:";
                        message += "\n    cppcheck --suppressions-list=a.txt --suppressions-list=b.txt file.cpp";
                    }

                    PrintMessage(message);
                    return false;
                }
                const std::string errmsg(_settings->nomsg.parseFile(f));
                if (!errmsg.empty()) {
                    PrintMessage(errmsg);
                    return false;
                }
            }

            else if (std::strncmp(argv, "--suppress=", 11) == 0) {
                std::string suppression = argv+11;
                const std::string errmsg(_settings->nomsg.addSuppressionLine(suppression));
                if (!errmsg.empty()) {
                    PrintMessage(errmsg);
                    return false;
                }
            }

            // Enables inline suppressions.
            else if (std::strcmp(argv, "--inline-suppr") == 0)
                _settings->inlineSuppressions = true;

            // Verbose error messages (configuration info)
            else if (std::strcmp(argv, "-v") == 0 || std::strcmp(argv, "--verbose") == 0)
                _settings->verbose = true;

            // Force checking of files that have "too many" configurations
            else if (std::strcmp(argv, "-f") == 0 || std::strcmp(argv, "--force") == 0)
                _settings->force = true;

            // Output relative paths
            else if (std::strcmp(argv, "-rp") == 0 || std::strcmp(argv, "--relative-paths") == 0)
                _settings->relativePaths = true;
            else if (std::strncmp(argv, "-rp=", 4) == 0 || std::strncmp(argv, "--relative-paths=", 17) == 0) {
                _settings->relativePaths = true;
                if (argv[argv[3]=='='?4:17] != 0) {
                    std::string paths = argv+(argv[3]=='='?4:17);
                    for (;;) {
                        std::string::size_type pos = paths.find(';');
                        if (pos == std::string::npos) {
                            _settings->basePaths.push_back(Path::fromNativeSeparators(paths));
                            break;
                        } else {
                            _settings->basePaths.push_back(Path::fromNativeSeparators(paths.substr(0, pos)));
                            paths.erase(0, pos + 1);
                        }
                    }
                } else {
                    PrintMessage("cppcheck: No paths specified for the '" + std::string(argv) + "' option.");
                    return false;
                }
            }

            // Write results in results.xml
            else if (std::strcmp(argv, "--xml") == 0)
                _settings->xml = true;

            // Define the XML file version (and enable XML output)
            else if (std::strncmp(argv, "--xml-version=", 14) == 0) {
                std::string numberString(argv+14);

                std::istringstream iss(numberString);
                if (!(iss >> _settings->xml_version)) {
                    PrintMessage("cppcheck: argument to '--xml-version' is not a number.");
                    return false;
                }

                if (_settings->xml_version < 0 || _settings->xml_version > 2) {
                    // We only have xml versions 1 and 2
                    PrintMessage("cppcheck: '--xml-version' can only be 1 or 2.");
                    return false;
                }

                // Enable also XML if version is set
                _settings->xml = true;
            }

            // Only print something when there are errors
            else if (std::strcmp(argv, "-q") == 0 || std::strcmp(argv, "--quiet") == 0)
                _settings->quiet = true;

            // Append user-defined code to checked source code
            else if (std::strncmp(argv, "--append=", 9) == 0) {
                // This is deprecated and will be removed in 1.80
                PrintMessage("cppcheck: '--append' is deprecated and will be removed in version 1.80. To supply additional information to cppcheck, use --library or --include.");

                const std::string filename = 9 + argv;
                if (!_settings->append(filename)) {
                    PrintMessage("cppcheck: Couldn't open the file: \"" + filename + "\".");
                    return false;
                }
            }

            // Check configuration
            else if (std::strcmp(argv, "--check-config") == 0) {
                _settings->checkConfiguration = true;
            }

            // Check library definitions
            else if (std::strcmp(argv, "--check-library") == 0) {
                _settings->checkLibrary = true;
            }

            else if (std::strncmp(argv, "--enable=", 9) == 0) {
                const std::string errmsg = _settings->addEnabled(argv + 9);
                if (!errmsg.empty()) {
                    PrintMessage(errmsg);
                    return false;
                }
                // when "style" is enabled, also enable "warning", "performance" and "portability"
                if (_settings->isEnabled("style")) {
                    _settings->addEnabled("warning");
                    _settings->addEnabled("performance");
                    _settings->addEnabled("portability");
                }
            }

            // --error-exitcode=1
            else if (std::strncmp(argv, "--error-exitcode=", 17) == 0) {
                std::string temp = argv+17;
                std::istringstream iss(temp);
                if (!(iss >> _settings->exitCode)) {
                    _settings->exitCode = 0;
                    PrintMessage("cppcheck: Argument must be an integer. Try something like '--error-exitcode=1'.");
                    return false;
                }
            }

            // User define
            else if (std::strncmp(argv, "-D", 2) == 0) {
                std::string define;

                // "-D define"
                if (std::strcmp(argv, "-D") == 0) {

					if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: argument to '-D' is missing.");
                        return false;
                    }

                    define = aNext;
                }
                // "-Ddefine"
                else {
                    define = 2 + argv;
                }

                // No "=", append a "=1"
                if (define.find('=') == std::string::npos)
                    define += "=1";

                if (!_settings->userDefines.empty())
                    _settings->userDefines += ";";
                _settings->userDefines += define;

                def = true;
            }
            // User undef
            else if (std::strncmp(argv, "-U", 2) == 0) {
                std::string undef;

                // "-U undef"
                if (std::strcmp(argv, "-U") == 0) {

					if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: argument to '-U' is missing.");
                        return false;
                    }

                    undef = aNext;
                }
                // "-Uundef"
                else {
                    undef = 2 + argv;
                }

                _settings->userUndefs.insert(undef);
            }

            // -E
            else if (std::strcmp(argv, "-E") == 0) {
                _settings->preprocessOnly = true;
            }

            // Include paths
            else if (std::strncmp(argv, "-I", 2) == 0) {
                std::string path;

                // "-I path/"
                if (std::strcmp(argv, "-I") == 0) {

					if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: argument to '-I' is missing.");
                        return false;
                    }
                    path = aNext;
                }

                // "-Ipath/"
                else {
                    path = 2 + argv;
                }
                path = Path::removeQuotationMarks(path);
                path = Path::fromNativeSeparators(path);

                // If path doesn't end with / or \, add it
                if (path.back() != '/')
                    path += '/';

                _settings->includePaths.push_back(path);
            } else if (std::strncmp(argv, "--include=", 10) == 0) {
                std::string path = argv + 10;

                path = Path::fromNativeSeparators(path);

                _settings->userIncludes.push_back(path);
            } else if (std::strncmp(argv, "--includes-file=", 16) == 0) {
                // open this file and read every input file (1 file name per line)
                AddInclPathsToList(16 + argv, &_settings->includePaths);
            } else if (std::strncmp(argv, "--config-exclude=",17) ==0) {
                std::string path = argv + 17;
                path = Path::fromNativeSeparators(path);
                _settings->configExcludePaths.insert(path);
            } else if (std::strncmp(argv, "--config-excludes-file=", 23) == 0) {
                // open this file and read every input file (1 file name per line)
                AddPathsToSet(23 + argv, &_settings->configExcludePaths);
            }

            // file list specified
            else if (std::strncmp(argv, "--file-list=", 12) == 0) {
                // open this file and read every input file (1 file name per line)
                AddFilesToList(12 + argv, _pathnames);
            }

            // Ignored paths
            else if (std::strncmp(argv, "-i", 2) == 0) {
                std::string path;

                // "-i path/"
                if (std::strcmp(argv, "-i") == 0) {

					if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: argument to '-i' is missing.");
                        return false;
                    }
                    path = aNext;
                }

                // "-ipath/"
                else {
                    path = 2 + argv;
                }

                if (!path.empty()) {
                    path = Path::removeQuotationMarks(path);
                    path = Path::fromNativeSeparators(path);
                    path = Path::simplifyPath(path);

                    if (FileLister::isDirectory(path)) {
                        // If directory name doesn't end with / or \, add it
                        if (path.back() != '/')
                            path += '/';
                    }
                    _ignoredPaths.push_back(path);
                }
            }

            // --library
            else if (std::strncmp(argv, "--library=", 10) == 0) {
                if (!CppCheckExecutor::tryLoadLibrary(_settings->library, argv, argv+10))
                    return false;
            }

            // --project
            else if (std::strncmp(argv, "--project=", 10) == 0) {
                _settings->project.import(argv+10);
                if (std::strstr(argv, ".sln") || std::strstr(argv, ".vcxproj"))
                    CppCheckExecutor::tryLoadLibrary(_settings->library, argv, "windows");
            }

            // Report progress
            else if (std::strcmp(argv, "--report-progress") == 0) {
                _settings->reportProgress = true;
            }

            // --std
            else if (std::strcmp(argv, "--std=posix") == 0) {
                _settings->standards.posix = true;
            } else if (std::strcmp(argv, "--std=c89") == 0) {
                _settings->standards.c = Standards::C89;
            } else if (std::strcmp(argv, "--std=c99") == 0) {
                _settings->standards.c = Standards::C99;
            } else if (std::strcmp(argv, "--std=c11") == 0) {
                _settings->standards.c = Standards::C11;
            } else if (std::strcmp(argv, "--std=c++03") == 0) {
                _settings->standards.cpp = Standards::CPP03;
            } else if (std::strcmp(argv, "--std=c++11") == 0) {
                _settings->standards.cpp = Standards::CPP11;
            }
			/*
            // Output formatter
            else if (std::strcmp(argv, "--template") == 0 ||
                     std::strncmp(argv, "--template=", 11) == 0) {
                // "--template path/"
                if (argv[10] == '=')
                    _settings->outputFormat = argv + 11;
                else if ((i+1) < argc && argv[i+1][0] != '-') {
                    ++i;
                    _settings->outputFormat = argv;
                } else {
                    PrintMessage("cppcheck: argument to '--template' is missing.");
                    return false;
                }

                if (_settings->outputFormat == "gcc")
                    _settings->outputFormat = "{file}:{line}: {severity}: {message}";
                else if (_settings->outputFormat == "vs")
                    _settings->outputFormat = "{file}({line}): {severity}: {message}";
                else if (_settings->outputFormat == "edit")
                    _settings->outputFormat = "{file} +{line}: {severity}: {message}";
            }*/

            // Checking threads
            else if (std::strncmp(argv, "-j", 2) == 0) {
                std::string numberString;

                // "-j 3"
                if (std::strcmp(argv, "-j") == 0) {

					if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: argument to '-j' is missing.");
                        return false;
                    }

                    numberString = aNext;
                }

                // "-j3"
                else
                    numberString = argv+2;

                std::istringstream iss(numberString);
                if (!(iss >> _settings->jobs)) {
                    PrintMessage("cppcheck: argument to '-j' is not a number.");
                    return false;
                }

                if (_settings->jobs > 10000) {
                    // This limit is here just to catch typos. If someone has
                    // need for more jobs, this value should be increased.
                    PrintMessage("cppcheck: argument for '-j' is allowed to be 10000 at max.");
                    return false;
                }
            } else if (std::strncmp(argv, "-l", 2) == 0) {
                std::string numberString;

                // "-l 3"
                if (std::strcmp(argv, "-l") == 0) {

					if (aNext == 0 || aNext[0] == '-') {
                        PrintMessage("cppcheck: argument to '-l' is missing.");
                        return false;
                    }

                    numberString = aNext;
                }
				/*
                // "-l3"
                else
                    numberString = argv+2;
				*/

                std::istringstream iss(numberString);
                if (!(iss >> _settings->loadAverage)) {
                    PrintMessage("cppcheck: argument to '-l' is not a number.");
                    return false;
                }
            }

            // print all possible error messages..
            else if (std::strcmp(argv, "--errorlist") == 0) {
                _showErrorMessages = true;
                _settings->xml = true;
                _exitAfterPrint = true;
            }

            // documentation..
            else if (std::strcmp(argv, "--doc") == 0) {
                std::ostringstream doc;
                // Get documentation..
                for (std::list<Check *>::iterator it = Check::instances().begin(); it != Check::instances().end(); ++it) {
                    const std::string& name((*it)->name());
                    const std::string info((*it)->classInfo());
                    if (!name.empty() && !info.empty())
                        doc << "## " << name << " ##\n"
                            << info << "\n";
                }

                std::cout << doc.str();
                _exitAfterPrint = true;
                return true;
            }

            // show timing information..
            else if (std::strncmp(argv, "--showtime=", 11) == 0) {
                const std::string showtimeMode = argv + 11;
                if (showtimeMode == "file")
                    _settings->showtime = SHOWTIME_FILE;
                else if (showtimeMode == "summary")
                    _settings->showtime = SHOWTIME_SUMMARY;
                else if (showtimeMode == "top5")
                    _settings->showtime = SHOWTIME_TOP5;
                else if (showtimeMode.empty())
                    _settings->showtime = SHOWTIME_NONE;
                else {
                    std::string message("cppcheck: error: unrecognized showtime mode: \"");
                    message += showtimeMode;
                    message += "\". Supported modes: file, summary, top5.";
                    PrintMessage(message);
                    return false;
                }
            }

#ifdef HAVE_RULES
            // Rule given at command line
            else if (std::strncmp(argv, "--rule=", 7) == 0) {
                Settings::Rule rule;
                rule.pattern = 7 + argv;
                _settings->rules.push_back(rule);
            }

            // Rule file
            else if (std::strncmp(argv, "--rule-file=", 12) == 0) {
                tinyxml2::XMLDocument doc;
                if (doc.LoadFile(12+argv) == tinyxml2::XML_SUCCESS) {
                    tinyxml2::XMLElement *node = doc.FirstChildElement();
                    for (; node && strcmp(node->Value(), "rule") == 0; node = node->NextSiblingElement()) {
                        Settings::Rule rule;

                        tinyxml2::XMLElement *tokenlist = node->FirstChildElement("tokenlist");
                        if (tokenlist)
                            rule.tokenlist = tokenlist->GetText();

                        tinyxml2::XMLElement *pattern = node->FirstChildElement("pattern");
                        if (pattern) {
                            rule.pattern = pattern->GetText();
                        }

                        tinyxml2::XMLElement *message = node->FirstChildElement("message");
                        if (message) {
                            tinyxml2::XMLElement *severity = message->FirstChildElement("severity");
                            if (severity)
                                rule.severity = Severity::fromString(severity->GetText());

                            tinyxml2::XMLElement *id = message->FirstChildElement("id");
                            if (id)
                                rule.id = id->GetText();

                            tinyxml2::XMLElement *summary = message->FirstChildElement("summary");
                            if (summary)
                                rule.summary = summary->GetText() ? summary->GetText() : "";
                        }

                        if (!rule.pattern.empty())
                            _settings->rules.push_back(rule);
                    }
                }
            }
#endif

            // Specify platform
            else if (std::strncmp(argv, "--platform=", 11) == 0) {
                std::string platform(11+argv);

                if (platform == "win32A")
                    _settings->platform(Settings::Win32A);
                else if (platform == "win32W")
                    _settings->platform(Settings::Win32W);
                else if (platform == "win64")
                    _settings->platform(Settings::Win64);
                else if (platform == "unix32")
                    _settings->platform(Settings::Unix32);
                else if (platform == "unix64")
                    _settings->platform(Settings::Unix64);
                else if (platform == "native")
                    _settings->platform(Settings::Unspecified);
                else if (!_settings->platformFile(platform)) {
                    std::string message("cppcheck: error: unrecognized platform: \"");
                    message += platform;
                    message += "\".";
                    PrintMessage(message);
                    return false;
                }
            }

            // Set maximum number of #ifdef configurations to check
            else if (std::strncmp(argv, "--max-configs=", 14) == 0) {
                _settings->force = false;

                std::istringstream iss(14+argv);
                if (!(iss >> _settings->maxConfigs)) {
                    PrintMessage("cppcheck: argument to '--max-configs=' is not a number.");
                    return false;
                }

                if (_settings->maxConfigs < 1) {
                    PrintMessage("cppcheck: argument to '--max-configs=' must be greater than 0.");
                    return false;
                }

                maxconfigs = true;
            }

            // Print help
            else if (std::strcmp(argv, "-h") == 0 || std::strcmp(argv, "--help") == 0) {
                _pathnames.clear();
                _showHelp = true;
                _exitAfterPrint = true;
              //  break;
            }

            else {
                std::string message("cppcheck: error: unrecognized command line option: \"");
                message += argv;
                message += "\".";
                PrintMessage(message);
                return false;
            }
        }

        else {
            std::string path = Path::removeQuotationMarks(argv);
            path = Path::fromNativeSeparators(path);
            _pathnames.push_back(path);
        }

		if (_showHelp) {
			PrintHelp();
			return true;
		}

    return true;
}

void CmdLineParser::PrintHelp()
{
    std::cout << "Cppcheck - A tool for static C/C++ code analysis\n"
              "\n"
              "Syntax:\n"
              "    cppcheck [OPTIONS] [files or paths]\n"
              "\n"
              "If a directory is given instead of a filename, *.cpp, *.cxx, *.cc, *.c++, *.c,\n"
              "*.tpp, and *.txx files are checked recursively from the given directory.\n\n"
              "Options:\n"
              "    --cppcheck-build-dir=<dir>\n"
              "                         Analysis output directory. Useful for various data.\n"
              "                         Some possible usages are; whole program analysis,\n"
              "                         incremental analysis, distributed analysis.\n"
              "    --check-config       Check cppcheck configuration. The normal code\n"
              "                         analysis is disabled by this flag.\n"
              "    --check-library      Show information messages when library files have\n"
              "                         incomplete info.\n"
              "    --config-exclude=<dir>\n"
              "                         Path (prefix) to be excluded from configuration\n"
              "                         checking. Preprocessor configurations defined in\n"
              "                         headers (but not sources) matching the prefix will not\n"
              "                         be considered for evaluation.\n"
              "    --config-excludes-file=<file>\n"
              "                         A file that contains a list of config-excludes\n"
              "    --dump               Dump xml data for each translation unit. The dump\n"
              "                         files have the extension .dump and contain ast,\n"
              "                         tokenlist, symboldatabase, valueflow.\n"
              "    -D<ID>               Define preprocessor symbol. Unless --max-configs or\n"
              "                         --force is used, Cppcheck will only check the given\n"
              "                         configuration when -D is used.\n"
              "                         Example: '-DDEBUG=1 -D__cplusplus'.\n"
              "    -U<ID>               Undefine preprocessor symbol. Use -U to explicitly\n"
              "                         hide certain #ifdef <ID> code paths from checking.\n"
              "                         Example: '-UDEBUG'\n"
              "    -E                   Print preprocessor output on stdout and don't do any\n"
              "                         further processing.\n"
              "    --enable=<id>        Enable additional checks. The available ids are:\n"
              "                          * all\n"
              "                                  Enable all checks. It is recommended to only\n"
              "                                  use --enable=all when the whole program is\n"
              "                                  scanned, because this enables unusedFunction.\n"
              "                          * warning\n"
              "                                  Enable warning messages\n"
              "                          * style\n"
              "                                  Enable all coding style checks. All messages\n"
              "                                  with the severities 'style', 'performance' and\n"
              "                                  'portability' are enabled.\n"
              "                          * performance\n"
              "                                  Enable performance messages\n"
              "                          * portability\n"
              "                                  Enable portability messages\n"
              "                          * information\n"
              "                                  Enable information messages\n"
              "                          * unusedFunction\n"
              "                                  Check for unused functions. It is recommend\n"
              "                                  to only enable this when the whole program is\n"
              "                                  scanned.\n"
              "                          * missingInclude\n"
              "                                  Warn if there are missing includes. For\n"
              "                                  detailed information, use '--check-config'.\n"
              "                         Several ids can be given if you separate them with\n"
              "                         commas. See also --std\n"
              "    --error-exitcode=<n> If errors are found, integer [n] is returned instead of\n"
              "                         the default '0'. '" << EXIT_FAILURE << "' is returned\n"
              "                         if arguments are not valid or if no input files are\n"
              "                         provided. Note that your operating system can modify\n"
              "                         this value, e.g. '256' can become '0'.\n"
              "    --errorlist          Print a list of all the error messages in XML format.\n"
              "    --doc                Print a list of all available checks.\n"
              "    --exitcode-suppressions=<file>\n"
              "                         Used when certain messages should be displayed but\n"
              "                         should not cause a non-zero exitcode.\n"
              "    --file-list=<file>   Specify the files to check in a text file. Add one\n"
              "                         filename per line. When file is '-,' the file list will\n"
              "                         be read from standard input.\n"
              "    -f, --force          Force checking of all configurations in files. If used\n"
              "                         together with '--max-configs=', the last option is the\n"
              "                         one that is effective.\n"
              "    -h, --help           Print this help.\n"
              "    -I <dir>             Give path to search for include files. Give several -I\n"
              "                         parameters to give several paths. First given path is\n"
              "                         searched for contained header files first. If paths are\n"
              "                         relative to source files, this is not needed.\n"
              "    --includes-file=<file>\n"
              "                         Specify directory paths to search for included header\n"
              "                         files in a text file. Add one include path per line.\n"
              "                         First given path is searched for contained header\n"
              "                         files first. If paths are relative to source files,\n"
              "                         this is not needed.\n"
              "    --include=<file>\n"
              "                         Force inclusion of a file before the checked file. Can\n"
              "                         be used for example when checking the Linux kernel,\n"
              "                         where autoconf.h needs to be included for every file\n"
              "                         compiled. Works the same way as the GCC -include\n"
              "                         option.\n"
              "    -i <dir or file>     Give a source file or source file directory to exclude\n"
              "                         from the check. This applies only to source files so\n"
              "                         header files included by source files are not matched.\n"
              "                         Directory name is matched to all parts of the path.\n"
              "    --inconclusive       Allow that Cppcheck reports even though the analysis is\n"
              "                         inconclusive.\n"
              "                         There are false positives with this option. Each result\n"
              "                         must be carefully investigated before you know if it is\n"
              "                         good or bad.\n"
              "    --inline-suppr       Enable inline suppressions. Use them by placing one or\n"
              "                         more comments, like: '// cppcheck-suppress warningId'\n"
              "                         on the lines before the warning to suppress.\n"
              "    -j <jobs>            Start <jobs> threads to do the checking simultaneously.\n"
#ifdef THREADING_MODEL_FORK
              "    -l <load>            Specifies that no new threads should be started if\n"
              "                         there are other threads running and the load average is\n"
              "                         at least <load>.\n"
#endif
              "    --language=<language>, -x <language>\n"
              "                         Forces cppcheck to check all files as the given\n"
              "                         language. Valid values are: c, c++\n"
              "    --library=<cfg>      Load file <cfg> that contains information about types\n"
              "                         and functions. With such information Cppcheck\n"
              "                         understands your your code better and therefore you\n"
              "                         get better results. The std.cfg file that is\n"
              "                         distributed with Cppcheck is loaded automatically.\n"
              "                         For more information about library files, read the\n"
              "                         manual.\n"
              "    --project=<file>     Run Cppcheck on project. The <file> can be a Visual\n"
              "                         Studio Solution (*.sln), Visual Studio Project\n"
              "                         (*.vcxproj), or compile database\n"
              "                         (compile_commands.json). The files to analyse,\n"
              "                         include paths, defines, platform and undefines in\n"
              "                         the specified file will be used.\n"
              "    --max-configs=<limit>\n"
              "                         Maximum number of configurations to check in a file\n"
              "                         before skipping it. Default is '12'. If used together\n"
              "                         with '--force', the last option is the one that is\n"
              "                         effective.\n"
              "    --platform=<type>, --platform=<file>\n"
              "                         Specifies platform specific types and sizes. The\n"
              "                         available builtin platforms are:\n"
              "                          * unix32\n"
              "                                 32 bit unix variant\n"
              "                          * unix64\n"
              "                                 64 bit unix variant\n"
              "                          * win32A\n"
              "                                 32 bit Windows ASCII character encoding\n"
              "                          * win32W\n"
              "                                 32 bit Windows UNICODE character encoding\n"
              "                          * win64\n"
              "                                 64 bit Windows\n"
              "                          * native\n"
              "                                 Unspecified platform. Type sizes of host system\n"
              "                                 are assumed, but no further assumptions.\n"
              "    -q, --quiet          Do not show progress reports.\n"
              "    -rp, --relative-paths\n"
              "    -rp=<paths>, --relative-paths=<paths>\n"
              "                         Use relative paths in output. When given, <paths> are\n"
              "                         used as base. You can separate multiple paths by ';'.\n"
              "                         Otherwise path where source files are searched is used.\n"
              "                         We use string comparison to create relative paths, so\n"
              "                         using e.g. ~ for home folder does not work. It is\n"
              "                         currently only possible to apply the base paths to\n"
              "                         files that are on a lower level in the directory tree.\n"
              "    --report-progress    Report progress messages while checking a file.\n"
#ifdef HAVE_RULES
              "    --rule=<rule>        Match regular expression.\n"
              "    --rule-file=<file>   Use given rule file. For more information, see: \n"
              "                         http://sourceforge.net/projects/cppcheck/files/Articles/\n"
#endif
              "    --std=<id>           Set standard.\n"
              "                         The available options are:\n"
              "                          * posix\n"
              "                                 POSIX compatible code\n"
              "                          * c89\n"
              "                                 C code is C89 compatible\n"
              "                          * c99\n"
              "                                 C code is C99 compatible\n"
              "                          * c11\n"
              "                                 C code is C11 compatible (default)\n"
              "                          * c++03\n"
              "                                 C++ code is C++03 compatible\n"
              "                          * c++11\n"
              "                                 C++ code is C++11 compatible (default)\n"
              "                         More than one --std can be used:\n"
              "                           'cppcheck --std=c99 --std=posix file.c'\n"
              "    --suppress=<spec>    Suppress warnings that match <spec>. The format of\n"
              "                         <spec> is:\n"
              "                         [error id]:[filename]:[line]\n"
              "                         The [filename] and [line] are optional. If [error id]\n"
              "                         is a wildcard '*', all error ids match.\n"
              "    --suppressions-list=<file>\n"
              "                         Suppress warnings listed in the file. Each suppression\n"
              "                         is in the same format as <spec> above.\n"
              "    --template='<text>'  Format the error messages. E.g.\n"
              "                         '{file}:{line},{severity},{id},{message}' or\n"
              "                         '{file}({line}):({severity}) {message}' or\n"
              "                         '{callstack} {message}'\n"
              "                         Pre-defined templates: gcc, vs, edit.\n"
              "    -v, --verbose        Output more detailed error information.\n"
              "    --version            Print out version number.\n"
              "    --xml                Write results in xml format to error stream (stderr).\n"
              "    --xml-version=<version>\n"
              "                         Select the XML file version. Currently versions 1 and\n"
              "                         2 are available. The default version is 1."
              "\n"
              "Example usage:\n"
              "  # Recursively check the current folder. Print the progress on the screen and\n"
              "  # write errors to a file:\n"
              "  cppcheck . 2> err.txt\n"
              "\n"
              "  # Recursively check ../myproject/ and don't print progress:\n"
              "  cppcheck --quiet ../myproject/\n"
              "\n"
              "  # Check test.cpp, enable all checks:\n"
              "  cppcheck --enable=all --inconclusive --std=posix test.cpp\n"
              "\n"
              "  # Check f.cpp and search include files from inc1/ and inc2/:\n"
              "  cppcheck -I inc1/ -I inc2/ f.cpp\n"
              "\n"
              "For more information:\n"
              "    http://cppcheck.net/manual.pdf\n";
}
