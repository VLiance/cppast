
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
#include "cppcheck.h"


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


ObjFile::ObjFile( Settings& settings, ErrorLogger *errorLogger,CppCheck* _oParent):Preproc(settings, errorLogger),oTokenizer(0), settings(settings), errorLogger(errorLogger),tokens1(0), bTokennized(false), bReaded(false)
{
    oParent = _oParent;
}

void ObjFile::fIni(const std::string& _sFile){

	sFile = _sFile;
	oPreproc = &Preproc;
	oTokenizer = new Tokenizer(&settings, errorLogger);


  //  oFileStream = new std::ifstream((sFile.c_str()));

    sFileName = sFile;
    fExtractFileName(sFileName);

}


void ObjFile::fDelete(){
    bTokennized = false;

	if(oTokenizer != 0){
		delete oTokenizer;
	}
	if(tokens1 != 0){
		delete tokens1;
	}
}

ObjFile::~ObjFile(){
    fDelete();

}


void ObjFile::fReload(){
    //TODO
    fDelete();

    oPreproc->aIncludesList.clear();

    oTokenizer = new Tokenizer(&settings, errorLogger);


    fReadFileToToken();
   if( fTokenize()){
    fLoadAllInclude();
    fOutputClass();
   }

}


void ObjFile::fReparse(int _nFromLine, int _nToLine, const char* _sText){



    if(_nFromLine == 0  ){ //Reparce ALL

          fDelete();




        oPreproc->aIncludesList.clear();


        oTokenizer = new Tokenizer(&settings, errorLogger);


//settings.teminate(false);

        fReadBufferToToken(_sText,_nToLine);

        //fReadBufferToToken(sTest,  _nToLine);

         if( fTokenize()){



            fLoadAllInclude();

            fOutputClass();
         }


    }


}


void ObjFile::fReadFileToToken(){

Settings::terminate(false);

     std::ifstream _oFileStream;
     _oFileStream.open(sFile.c_str());
     if(_oFileStream.is_open()){
        printf("\nOPEN: %s",sFile.c_str());
        tokens1 = new simplecpp::TokenList(_oFileStream, files, sFile, &outputList);
        _oFileStream.close();
     }else{
         printf("\nError Openning: %s", sFile.c_str());
     }

}

class membuf : public std::streambuf
{
    public:
        membuf(char* p, size_t n) {
        setg(p, p, p + n);
         setp(p, p + n);
    }
};


void ObjFile::fReadBufferToToken( const char* _aBuff , unsigned int _nLenght){

    Settings::terminate(false);

     membuf _buff(( char*)_aBuff, _nLenght );

    std::istream istr(&_buff);
     tokens1 = new simplecpp::TokenList(istr, files, sFile, &outputList);

}



bool ObjFile::fTokenize(){


	//Tokenizer* _tokenizer =  oTokenizer;

	//	std::ifstream fileStream(sFile.c_str());



//	oFileStream = new std::ifstream((sFile.c_str())); //Open?
      // tokens1 = new simplecpp::TokenList(*oFileStream, files, sFile, &outputList);

 printf("\n-*********fil %d", files.size());



        oPreproc->loadFiles(*tokens1, files);

 printf("\n-*********fil2 %d", files.size());
			///All other files
				   for (unsigned int i = 0; i < files.size(); ++i) {
                         printf("\n-*********fil4 %d", files.size());
					printf("\n-files: |%s|",  files[i].c_str());
				}
 printf("\n-*********fil3 %d", files.size());
        // Get directives
        oPreproc->setDirectives(*tokens1);

        oPreproc->setPlatformInfo(tokens1);
printf("\n-*********fil4");
        configurations.clear();
        configurations.insert(settings.userDefines);  //Get user define!
		cfg  = *configurations.begin();

printf("\n-*********fil5");

        // write dump file xml prolog


     //   std::set<unsigned long long> checksums;
    //    unsigned int checkCount = 0;
     //   for (std::set<std::string>::const_iterator it = configurations.begin(); it != configurations.end(); ++it) { //Only one?

            // bail out if terminated
            if (settings.terminated()){
                    printf("\nFAIIL");
                return false;
            }

/*
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
printf("\n-*********fil6");




             codeWithoutCfg = oPreproc->getcode(*tokens1, cfg, files, true);


          //  }

            codeWithoutCfg += settings.append();
printf("\n-*********fil7");

            try {

                //CW build

                // Create tokens, skip rest of iteration if failed
               istr = std::istringstream(codeWithoutCfg);
               printf("\n-*********a1");
             //   Timer timer("Tokenizer::createTokens", settings.showtime, &S_timerResults);
                bool result = oTokenizer->createTokens(istr, sFile);
            //    timer.Stop();
          printf("\n-*********a2");
			//	S_timerResults.ShowResults(SHOWTIME_FILE);


                // Simplify tokens into normal form, skip rest of iteration if failed
              //  Timer timer2("Tokenizer::simplifyTokens1", settings.showtime, &S_timerResults);
                result = oTokenizer->simplifyTokens1(cfg);
printf("\n-*********a3");


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

			return true;

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

        bool verbose = true;
        std::string _sErr =  "cwcAst:A:|ErrMsg";
        _sErr.append("|CallStack:");
        if (!errmsg._callStack.empty()) {
            _sErr.append("*File:"); _sErr.append(errmsg._callStack.back().getfile().c_str());
            _sErr.append("*Line:"); _sErr.append( std::to_string( errmsg._callStack.back().line)) ;
            _sErr.append("<");
        }
       _sErr.append("|Type:");  _sErr.append( errmsg._id.c_str());
        _sErr.append("|Severity:");  _sErr.append( (errmsg._severity == Severity::error ? "error" : "style") );
        _sErr.append("|Msg:"); _sErr.append((verbose ? errmsg.verboseMessage() : errmsg.shortMessage()).c_str());
        std::cout <<_sErr << std::endl;

         //   std::cout <<  "cwcAst:A:|ErrMsg|" << errmsg.toXML(true, 1) << std::endl;
          //  std::cout <<  "cwcAst:A:|ErrMsg|" << errmsg.toXML(true, 2) << std::endl;

            //  Check::reportError(errmsg);
              return false;
        }
	//}


//	fSeekFirstTokenOnLine(10); //Temp

}



 simplecpp::Token*  ObjFile::fSeekFirstTokenOnLine(int _nLine){//
//const simplecpp::Token* cmdtok =  //First token?
std::cout  << "\n";
std::cout  << "\n";
  for (const simplecpp::Token *tok = tokens1->cfront(); tok; tok = tok->next) {
        if(tok->location.line == _nLine){
            std::cout << tok->str;
        }

  }
std::cout  << "\n";
return 0;//TODO ?
}


/*
bool sameline(const simplecpp::Token *tok1, const simplecpp::Token *tok2){
    return tok1 && tok2 && tok1->location.sameline(tok2->location);
}
static const simplecpp::Token* fSeekToken(const simplecpp::Token* cmdtok){
    //int level = 0;
    while (nullptr != (cmdtok = cmdtok->next)) {
        if (cmdtok->op == '#' && !sameline(cmdtok->previous,cmdtok) && sameline(cmdtok, cmdtok->next)) { //Start with #


            if (cmdtok->next->str.compare(0,2,"if")==0)
                ++level;
            else if (cmdtok->next->str == "endif") {
                --level;
                if (level < 0)
                    return cmdtok;
            }
        }
    }
    return nullptr;
}*/





void ObjFile::fLoadAllInclude(){
//Load recursively
    aIncludeObj.clear();
    for (unsigned int i = 0; i <  oPreproc->aIncludesList.size(); ++i) {
        ObjFile* _oFile = oParent->fReadFile(oPreproc->aIncludesList[i] );
        if(_oFile != nullptr){
            aIncludeObj.push_back( _oFile );
        }
    }
}
void ObjFile::fOutputClass(){
  printf("\n%s\n", fGetAllFunctions().c_str());

}

std::string ObjFile::fGetAllFunctions(){

//oTokenizer->_symbolDatabase->printOut("aaaa");


    std::vector<ObjFile*> _aFiles;


    std::string _sOut = "cwcAst:A:|ClassInfo|";
   _sOut.append(sFile);
   _sOut.append("|");

   _sOut.append(sFileName);  ///Current expression
   _sOut.append(":FPath:0#");  ///Current expression file? current class??
  // _sOut.append("GZ.Sys/Debug,3,Sys/;#");  ///Extend info ??
   _sOut.append(sFileName);  ///Extend info ??
   _sOut.append(",3,Sys/;#");  ///Extend info ??

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

        _sOut.append(_oObj->fGetMembers());
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

//  std::string result = "Cpp.";
  std::string result = "";
    result.append(sFileName);
    result.append(",0,");
    result.append(sFile);

    result.append(",");
    sImportIndex = std::to_string(_nIndex);

	result.append(sImportIndex);

    result.append(";");
    return result;
}

std::string ObjFile::fGetMembers(){
    std::list<Scope>* scopeList = &oTokenizer->_symbolDatabase->scopeList;

	if(!bTokennized || scopeList->size() == 0){
        return "";
    }

    return fGetMember(&*scopeList->begin());
}

std::string ObjFile::fGetMember(const Scope* scope, int _nMinLine ){

    std::string result = "";
    for (std::list<Variable>::const_iterator variable = scope->varlist.begin(); variable != scope->varlist.end(); ++variable) {

            int _nLine = variable->nameToken()->linenr();
            if(_nMinLine != 0 && _nMinLine < _nLine  ){
                continue;//Skip
            }

            const Token* oVarTok =  variable->nameToken();
            if(_nMinLine == 0){ //Only global
                result.append( sFileName);//Not optimal
                result.append( ".");
            }
			//	result.append( "Function");
			result.append( variable->name());
			//result <<=  function->name() << "&";
            result.append(":");

         //   ArgumentInfo argInfo(argListTok, _settings, _tokenizer->isCPP()); //TODO
            result.append(variable->typeStartToken()->str());

			result.append(":");

		//	if( function->token != nullptr){

					std::string _sLine = std::to_string(_nLine);

					result.append(_sLine);
            /*
			}else{
				result.append("0");
			}*/

			result.append(":Pb");

            result.append(":");
            result.append(sImportIndex);

            result.append(";");
		 }

    return result;
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



 //       printf("\ncppc:Func:%s" , function->name().c_str());

        result.append( sFileName);//Not optimal
        result.append( ".");

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
            	result.append("&");
            	//return
               // result.append(oFuncTok->() );
              const Token* typeToken = function->retDef;

/*
              if(function->retType){
                      typeToken = function->retType->classScope->enumType;
                result.append( typeToken->str() );
              }*/
/*
                 if(function->retType){
                             result.append("ASS");
               if (function->retType->classScope->enumType){
                            typeToken = function->retType->classScope->enumType;
                             result.append(typeToken->str() );
                        result.append(" ");
               }}*/


                bool bRetFound = false;
                    if (function->retType) {
                        bRetFound = true;
                        result.append("retType");

                    }else

              if (function->retDef) {

                    const Token *type = function->retDef;
                    while (Token::Match(type, "include|extern|static|const|struct|union|enum"))
                    type = type->next();

                    while (type && type->isName() && type != function->tokenDef ) {
                        result.append(type->str() );
                        result.append(" ");
                        bRetFound = true;
                          type = type->next();
                    }
                }
                if(!bRetFound){
                    result.append("void");
                }

/*
                if(  function->type ){
                                  result.append(function->type->str() );
                        result.append(" ");
                }*/

                /*
                while (typeToken->str() == "const" || typeToken->str() == "extern"){
                    typeToken = typeToken->next();
                }
                while (typeToken != end){
                    result.append(typeToken->str() );

                     result.append(" ");
                    typeToken = typeToken->next();
                }*/


                /*
                while (typeToken){

                }*/


                    //Param
                result.append("&");


                for (unsigned int argnr = 0; argnr < function->argCount(); ++argnr) {
                    const Variable *arg = function->getArgumentVar(argnr);
                    result.append(arg->name());

                      result.append(":");
                      result.append(arg->typeStartToken()->str() );

                      /*
                      if(arg->type() != nullptr){
                           result.append(arg->type()->name());
                      }*/

                    //   result.append(arg->type()); //Type

                   // out << "          <arg nr=\"" << argnr+1 << "\" variable=\"" << arg << "\"/>" << std::endl;
                   result.append(",");
                }

             	// result.append("_sValue:String:115");

		//	result.append("&Void&_sValue:String:115");

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




void ObjFile::fGetLocalScopeVar(int _nLine, int _nLastLine){
  //  printf("\nOKAY : %d", _nLine );
   // printf("\nOKAYLASr : %d", _nLastLine );

	 std::list<Scope>* scopeList = &oTokenizer->_symbolDatabase->scopeList;
    if(!bTokennized || scopeList->size() == 0){
        return ;
    }

    //Function name

//    result.append("Test:5:10");

 std::string className;

 std::string sMember;
//printf("\n-----------Try to scopelist");
    bool _bFound = false;
    for (std::list<Scope>::const_iterator scope = scopeList->begin(); scope != scopeList->end(); ++scope) {
       // unsigned int _nStart =
      //  std::list<Function> _aFunc =  scope->functionList;
        if(scope->classStart && scope->classEnd){
           if(_nLine >= scope->classStart->linenr() &&   _nLine <= scope->classEnd->linenr()){
                printf("\nScope : %d : %d",  scope->classStart->linenr() , scope->classEnd->linenr());
               if( scope->className != ""){
                className = scope->className;
               }
               _bFound = true;
                sMember.append(fGetMember(&*scope,_nLine));
           }
        }
    }


    std::string result = "cwcAst:A:|LocalScopeInfo|";
    result.append(sFile);
    result.append("|");

    if(_bFound){
        result.append(className);
        result.append(":5:10");
        result.append("|");
        result.append(sMember);

    }else{
        result = "cwcAst:A:|LocalScopeOut|";
        result.append(sFile);
        result.append("|");
    }
    printf("\n%s\n", result.c_str());

}





