
#include <iostream>
#include "ObjFile.h"
#include "preprocessor.h"
#include "path.h"
#include "errorlogger.h"
#include "settings.h"
#include "simplecpp.h"

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

static TimerResults S_timerResults;


ObjFile::ObjFile( Settings& settings, ErrorLogger *errorLogger):Preproc(settings, errorLogger),oTokenizer(0), settings(settings), errorLogger(errorLogger),tokens1(0), bTokennized(false)
{

}

void ObjFile::fIni(const std::string& _sFile){

	sFile = _sFile;
	oPreproc = &Preproc;
	oTokenizer = new Tokenizer(&settings, errorLogger);
	oFileStream = new std::ifstream((sFile.c_str()));

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

			///All other files
				   for (unsigned int i = 0; i < files.size(); ++i) {
					printf("\n-*********files: %s",  files[i].c_str());
				}

        // Get directives
        oPreproc->setDirectives(*tokens1);

        oPreproc->setPlatformInfo(tokens1);

        configurations.insert(settings.userDefines);  //Get user define!
		cfg  = *configurations.begin();


/*
        // write dump file xml prolog
        std::ofstream fdump;
        if (_settings.dump) {
            const std::string dumpfile(filename + ".dump");
            fdump.open(dumpfile.c_str());
            if (fdump.is_open()) {
                fdump << "<?xml version=\"1.0\"?>" << std::endl;
                fdump << "<dumps>" << std::endl;
            }
        }*/

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

			printf("\n\n-------ProcessConfig: %s\n\n\n" ,cfg.c_str());
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







/*
            if (_settings.showtime != SHOWTIME_NONE)
                _tokenizer->setTimerResults(&S_timerResults);*/

		//	 std::string codeWithoutCfg;
       //     {
            //    Timer t("Preprocessor::getcode", settings.showtime, &S_timerResults);
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

              //  timer2.Stop();


		//	fSendFunctions();

			//	S_timerResults.ShowResults(SHOWTIME_FILE);

  //     std::ofstream fdump;
//	  _tokenizer->dump(fdump);




/*
            // dump xml if --dump
            if (_settings.dump && fdump.is_open()) {
                fdump << "<dump cfg=\"" << cfg << "\">" << std::endl;

                preprocessor->dump(fdump);
                _tokenizer.dump(fdump);

                fdump << "</dump>" << std::endl;
            }
*/
			bTokennized = true;
			///All other files
			for (unsigned int i = 0; i <  oPreproc->aIncludesList.size(); ++i) {
				printf("\n-------files: %s",  oPreproc->aIncludesList[i].c_str());
			}

/*
			for (unsigned int i = 0; i <  preprocessor->aIncludesList.size(); ++i) {
				fReadFile(preprocessor->aIncludesList[i]);
			}*/

		//	return this;

        } catch (const InternalError &e) {
/*
            std::list<ErrorLogger::ErrorMessage::FileLocation> locationList;
            ErrorLogger::ErrorMessage::FileLocation loc;
            if (e.token) {
                loc.line = e.token->linenr();
                const std::string fixedpath = Path::toNativeSeparators(_tokenizer->list.file(e.token));
                loc.setfile(fixedpath);
            } else {
                ErrorLogger::ErrorMessage::FileLocation loc2;
                loc2.setfile(Path::toNativeSeparators(filename));
                locationList.push_back(loc2);
                loc.setfile(_tokenizer->list.getSourceFilePath());
            }
            locationList.push_back(loc);
            ErrorLogger::ErrorMessage errmsg(locationList,
                                                _tokenizer->list.getSourceFilePath(),
                                                Severity::error,
                                                e.errorMessage,
                                                e.id,
                                                false);
            reportErr(errmsg);*/
        }
	//}
}


#include <sstream>
#include <string>

void ObjFile::fSendFunctions(){

if(!bTokennized){
	return;
}

  std::string result = "#";


	 std::list<Scope>* scopeList = &oTokenizer->_symbolDatabase->scopeList;
printf("\nTry to scopelist");

	for (std::list<Scope>::const_iterator scope = scopeList->begin(); scope != scopeList->end(); ++scope) {


      for (std::list<Function>::const_iterator function = scope->functionList.begin(); function != scope->functionList.end(); ++function) {
			if( function->token == nullptr){
				continue;
			}
			 const Token* oFuncTok =  function->token;


		// printf("\ncppc:Func:%s" , function->name().c_str());
			 printf("\ncppc:Func:%s" , function->name().c_str());
			// printf("\ncppc:Func:%s" , function->.c_str());
				// printf("\n--cppc:Func:" );

			//	result.append( "Function");
			result.append( function->name());
			//result <<=  function->name() << "&";

			result.append("&");
/*
		//	if( function->token != nullptr){
					int _nLine = oFuncTok->linenr();
					std::string _sLine = std::to_string(_nLine);

					result.append(_sLine);
/*
			}else{
				result.append("0");
			}*/


			result.append("&Void&_sValue:String:115,;");


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

		 }
	}

  std::cout  << '\n' << result << '\n';

}
