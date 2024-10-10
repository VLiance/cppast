
#include <iostream>
#include "ObjFile.h"
#include "preprocessor.h"
#include "path.h"
#include "errorlogger.h"
#include "settings.h"
#include "simplecpp.h"

#include <sstream>
#include <string>

#include "check.h"
#include <algorithm>
#include <functional>
#include <sstream>
#include <cstdlib>
#include <cctype>
#include <vector>
#include <set>
#include <cstdint>
#include <fstream>
#include <sstream>
#include <stdexcept>
#include "timer.h"
#include "version.h"
#include "symboldatabase.h"


inline void fExtractFileName(std::string& _sFilePath){
    // Remove directory if present.
    // Do this before extension removal incase directory has a period character.
    const size_t last_slash_idx = _sFilePath.find_last_of("\\/");
    if (std::string::npos != last_slash_idx)
    {
        _sFilePath.erase(0, last_slash_idx + 1);
    }

    // Remove extension if present.
    const size_t period_idx = _sFilePath.rfind('.');
    if (std::string::npos != period_idx)
    {
        _sFilePath.erase(period_idx);
    }
}

/*
inline void decomposePath(const char *filePath, char *fileDir, char *fileName, char *fileExt)
{
    #if defined _WIN32
        const char *lastSeparator = strrchr(filePath, '\\');
    #else
        const char *lastSeparator = strrchr(filePath, '/');
    #endif

    const char *lastDot = strrchr(filePath, '.');
    const char *endOfPath = filePath + strlen(filePath);
    const char *startOfName = lastSeparator ? lastSeparator + 1 : filePath;
    const char *startOfExt = lastDot > startOfName ? lastDot : endOfPath;

    if(fileDir)
        _snprintf(fileDir, MAX_PATH, "%.*s", startOfName - filePath, filePath);

    if(fileName)
        _snprintf(fileName, MAX_PATH, "%.*s", startOfExt - startOfName, startOfName);

    if(fileExt)
        _snprintf(fileExt, MAX_PATH, "%s", startOfExt);
}*/



static TimerResults S_timerResults;


ObjFile::ObjFile( Settings& settings, ErrorLogger *errorLogger):Preproc(settings, errorLogger),oTokenizer(0), settings(settings), errorLogger(errorLogger),tokens1(0), bTokennized(false), bReaded(false)
{

}

void ObjFile::fIni(const std::string& _sFile){

	sFile = _sFile;
	oPreproc = &Preproc;
	oTokenizer = new Tokenizer(&settings, errorLogger);
	oFileStream = new std::ifstream((sFile.c_str()));

    sFileName = sFile;
    fExtractFileName(sFileName);

}



ObjFile::~ObjFile()
{
	if(oTokenizer != 0){
		delete oTokenizer;
		delete oFileStream;
	}
	if(tokens1 != 0){
		delete tokens1;
	}

}


void ObjFile::fTokenize(){
	//Tokenizer* _tokenizer =  oTokenizer;

	//	std::ifstream fileStream(sFile.c_str());



       tokens1 = new simplecpp::TokenList(*oFileStream, files, sFile, &outputList);



        oPreproc->loadFiles(*tokens1, files);
/*
			///All other files
				   for (unsigned int i = 0; i < files.size(); ++i) {
					printf("\n-*********files: %s",  files[i].c_str());
				}*/

        // Get directives
        oPreproc->setDirectives(*tokens1);

        oPreproc->setPlatformInfo(tokens1);

        configurations.insert(settings.userDefines);  //Get user define!
		cfg  = *configurations.begin();



        // write dump file xml prolog


     //   std::set<unsigned long long> checksums;
    //    unsigned int checkCount = 0;
     //   for (std::set<std::string>::const_iterator it = configurations.begin(); it != configurations.end(); ++it) { //Only one?
/*
            // bail out if terminated
            if (settings.terminated())
                break;

            // Check only a few configurations (default 12), after that bail out, unless --force
            // was used.
            if (!settings.force && ++checkCount > settings.maxConfigs)
                break;
*/
    //      auto cfg = *it;

//			printf("\n\n-------ProcessConfig: %s\n\n\n" ,cfg.c_str());
/*

            // If only errors are printed, print filename after the check
            if (settings.quiet == false && it != configurations.begin()) {
                std::string fixedpath = Path::simplifyPath(sFile);
                fixedpath = Path::toNativeSeparators(fixedpath);
                errorLogger.reportOut("Checking " + fixedpath + ": " + cfg + "...");
            }*/

            if (!settings.userDefines.empty()) {
                if (!cfg.empty())
                    cfg = ";" + cfg;
                cfg = settings.userDefines + cfg;
            }




             codeWithoutCfg = oPreproc->getcode(*tokens1, cfg, files, true);


          //  }

            codeWithoutCfg += settings.append();

            try {

                //CW build

                // Create tokens, skip rest of iteration if failed
               istr = std::istringstream(codeWithoutCfg);
             //   Timer timer("Tokenizer::createTokens", settings.showtime, &S_timerResults);
                bool result = oTokenizer->createTokens(istr, sFile);
            //    timer.Stop();

			//	S_timerResults.ShowResults(SHOWTIME_FILE);


                // Simplify tokens into normal form, skip rest of iteration if failed
              //  Timer timer2("Tokenizer::simplifyTokens1", settings.showtime, &S_timerResults);
                result = oTokenizer->simplifyTokens1(cfg);



            // dump xml if --dump
         //   if (settings.dump && fdump.is_open()) {
         /*
        std::ofstream fdump;
        const std::string dumpfile(sFile + ".dump");
        fdump.open(dumpfile.c_str());
            if ( fdump.is_open()) {
                printf("-----------!!!!!!DUMP!!!!!! %s", sFile.c_str());
                fdump << "<dump cfg=\"" << cfg << "\">" << std::endl;
                oPreproc->dump(fdump);
                oTokenizer->dump(fdump);
                fdump << "</dump>" << std::endl;
            }
        */


			bTokennized = true;

			/*

			///All other files
			for (unsigned int i = 0; i <  oPreproc->aIncludesList.size(); ++i) {
				printf("\n-------files: %s",  oPreproc->aIncludesList[i].c_str());
			}
*/
/*
			for (unsigned int i = 0; i <  preprocessor->aIncludesList.size(); ++i) {
				fReadFile(preprocessor->aIncludesList[i]);
			}*/

		//	return this;

        } catch (const InternalError &e) {

            std::list<ErrorLogger::ErrorMessage::FileLocation> locationList;
            ErrorLogger::ErrorMessage::FileLocation loc;
            if (e.token) {
                loc.line = e.token->linenr();
                const std::string fixedpath = Path::toNativeSeparators(oTokenizer->list.file(e.token));
                loc.setfile(fixedpath);
            } else {
                ErrorLogger::ErrorMessage::FileLocation loc2;
                loc2.setfile(Path::toNativeSeparators(sFile));
                locationList.push_back(loc2);
                loc.setfile(oTokenizer->list.getSourceFilePath());
            }
            locationList.push_back(loc);
            ErrorLogger::ErrorMessage errmsg(locationList,
                                                oTokenizer->list.getSourceFilePath(),
                                                Severity::error,
                                                e.errorMessage,
                                                e.id,
                                                false);
              Check::reportError(errmsg);
        }
	//}
}



std::string ObjFile::fGetAllFunctions(){

//oTokenizer->_symbolDatabase->printOut("aaaa");


    std::vector<ObjFile*> _aFiles;


    std::string _sOut = "cwcAst:A:|ClassInfo|";
   _sOut.append(sFile);
   _sOut.append("|");

   _sOut.append(sFileName);  ///Current expression
   _sOut.append(":FPath:0#");  ///Current expression file? current class??
   _sOut.append("GZ.Sys/Debug,3,Sys/;#");  ///Extend info ??

    //List of import/includes
    //Folder . name, line, file


    fGetAllSubFile(_aFiles);

    /////All imports ///
    for (unsigned int i = 0; i <  _aFiles.size(); ++i) {
        ObjFile* _oObj = _aFiles[i];
        _sOut.append(_oObj->fGetImport(i+1));
    }
    _sOut.append("#");

    ////Member ///
    for (unsigned int i = 0; i <  _aFiles.size(); ++i) {
        ObjFile* _oObj = _aFiles[i];
        _sOut.append(_oObj->fGetMember());
    }
    _sOut.append("#");

    ////Functions ///
    for (unsigned int i = 0; i <  _aFiles.size(); ++i) {
        ObjFile* _oObj = _aFiles[i];
        _sOut.append(_oObj->fGetFunctions());
    }
   _sOut.append("#");

    ////Clear readed files
    for (unsigned int i = 0; i <  _aFiles.size(); ++i) {
        ObjFile* _oObj = _aFiles[i];
        _oObj->bReaded = false;
    }
    bReaded = false;
    _aFiles.clear();


    //disable also all readed sub file ...
    //TODO

    return _sOut;
}

std::string ObjFile::fGetAllSubFile(std::vector<ObjFile*>& _aFiles){

   // std::string _sOut = fGetFunctions();
    std::string _sOut ="";
    _aFiles.push_back(this);

    bReaded = true; //Todo disable all readed param

	//All others
    for (unsigned int i = 0; i <  aIncludeObj.size(); ++i) {
        ObjFile* _oObj = aIncludeObj[i];
        if(_oObj->bReaded == false){
            _sOut.append(_oObj->fGetAllSubFile(_aFiles));
        }
    }
    return _sOut;
}



std::string ObjFile::fGetImport(unsigned int _nIndex){


// printf("\ncppc:Scope:%s" , scope->className.c_str());
   nImportIndex = _nIndex;

  std::string result = "Cpp.";
    result.append(sFileName);
    result.append(",0,");
    result.append(sFile);

    result.append(",");
    sImportIndex = std::to_string(_nIndex);

	result.append(sImportIndex);

    result.append(";");
    return result;
}



std::string ObjFile::fGetMember(){


	 std::list<Scope>* scopeList = &oTokenizer->_symbolDatabase->scopeList;

    if(!bTokennized || scopeList->size() == 0){
        return "";
    }

  std::string result = "";


//printf("\n-----------Try to scopelist");

	for (std::list<Scope>::const_iterator scope = scopeList->begin(); scope != scopeList->end(); ++scope) { //BREAK Proceed only Global scope (frst one)

      for (std::list<Variable>::const_iterator variable = scope->varlist.begin(); variable != scope->varlist.end(); ++variable) {

            const Token* oVarTok =  variable->nameToken();

		// printf("\ncppc:Func:%s" , function->name().c_str());
//			 printf("\ncppc:Var:%s" , variable->name().c_str());
			// printf("\ncppc:Func:%s" , function->.c_str());
				// printf("\n--cppc:Func:" );

			//	result.append( "Function");
			result.append( variable->name());
			//result <<=  function->name() << "&";
            result.append(":String");
			result.append(":");

		//	if( function->token != nullptr){
					int _nLine = variable->nameToken()->linenr();
					std::string _sLine = std::to_string(_nLine);

					result.append(_sLine);
/*
			}else{
				result.append("0");
			}*/

			result.append(":Pb");


                  result.append(":");
              result.append(sImportIndex);

			//result.append( function->name().c_str());


/*
				 out << "        <function id=\"" << &*function << "\" tokenDef=\"" << function->tokenDef << "\" name=\"" << ErrorLogger::toxml(function->name()) << '\"';
                    if (function->argCount() == 0U)
                        out << "/>" << std::endl;
                    else {
                        out << ">" << std::endl;
                        for (unsigned int argnr = 0; argnr < function->argCount(); ++argnr) {
                            const Variable *arg = function->getArgumentVar(argnr);
                            out << "          <arg nr=\"" << argnr+1 << "\" variable=\"" << arg << "\"/>" << std::endl;
                        }
                        out << "        </function>" << std::endl;
                    }
                }
*/
                result.append(";");
		 }
        break; //Proceed only Global scope (frst one)
	}


    return result;
 // std::cout  << '\n' << result << '\n';
}

std::string ObjFile::fGetFunctions(){


	 std::list<Scope>* scopeList = &oTokenizer->_symbolDatabase->scopeList;



    if(!bTokennized || scopeList->size() == 0){
        return "";
    }

  std::string result = "";


//printf("\n-----------Try to scopelist");

	for (std::list<Scope>::const_iterator scope = scopeList->begin(); scope != scopeList->end(); ++scope) {

    std::list<Function> _aFunc =  scope->functionList;
//_aFunc = &oTokenizer->_symbolDatabase->functionList;

      for (std::list<Function>::const_iterator function = _aFunc.begin(); function != _aFunc.end(); ++function) {




        	if( function->tokenDef == nullptr){
				continue;
			}
			 const Token* oFuncTok =  function->tokenDef;



        printf("\ncppc:Func:%s" , function->name().c_str());


			result.append( function->name());
			//result <<=  function->name() << "&";

			result.append("&");

		//	if( function->token != nullptr){
					int _nLine = oFuncTok->linenr();
					std::string _sLine = std::to_string(_nLine);

					result.append(_sLine);
/*
			}else{
				result.append("0");
			}*/


			result.append("&Void&_sValue:String:115");

      result.append("&");
              result.append(sImportIndex);

			//result.append( function->name().c_str());


/*
				 out << "        <function id=\"" << &*function << "\" tokenDef=\"" << function->tokenDef << "\" name=\"" << ErrorLogger::toxml(function->name()) << '\"';
                    if (function->argCount() == 0U)
                        out << "/>" << std::endl;
                    else {
                        out << ">" << std::endl;
                        for (unsigned int argnr = 0; argnr < function->argCount(); ++argnr) {
                            const Variable *arg = function->getArgumentVar(argnr);
                            out << "          <arg nr=\"" << argnr+1 << "\" variable=\"" << arg << "\"/>" << std::endl;
                        }
                        out << "        </function>" << std::endl;
                    }
                }
*/
            result.append(";");
		 }
	}
	 //   result.append("#");
    return result;
 // std::cout  << '\n' << result << '\n';
}
