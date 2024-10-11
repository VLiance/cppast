#pragma once
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

//---------------------------------------------------------------------------
#ifndef ObjFileH
#define ObjFileH
//---------------------------------------------------------------------------


#include "simplecpp.h"
#include "preprocessor.h"
#include "tokenize.h" // Tokenizer
#include "settings.h"
#include <map>
#include <istream>
#include <string>
#include <list>
#include <set>

class ErrorLogger;
class Settings;
class CppCheck;
class Scope;

/**
 * @brief A preprocessor directive
 * Each preprocessor directive (\#include, \#define, \#undef, \#if, \#ifdef, \#else, \#endif)
 * will be recorded as an instance of this class.
 *
 * file and linenr denote the location where where the directive is defined.
 *
 */

class CPPCHECKLIB ObjFile {
public:
    bool bReaded ;

     std::vector<ObjFile*> aIncludeObj; //List of included headers

    unsigned int nImportIndex;
    std::string sImportIndex;
	bool bTokennized;
   simplecpp::TokenList* tokens1;
	  std::set<std::string> configurations;

        simplecpp::OutputList outputList;
        std::vector<std::string> files;

     std::istringstream istr;
   std::string  codeWithoutCfg;
    std::string cfg;
//std::ifstream* oFileStream;

CppCheck* oParent;
Preprocessor Preproc;
Preprocessor* oPreproc;
Tokenizer* oTokenizer;
Settings settings;
ErrorLogger *errorLogger;
std::string sFile;
std::string sFileName;

 std::vector<std::string> aIncludesList; //List of included headers


    ObjFile( Settings& settings,  ErrorLogger *errorLogger,  CppCheck *_oParent );
	void fIni(const std::string&);
	bool fTokenize();
	void fReadFileToToken();
    simplecpp::Token*  fSeekFirstTokenOnLine(int _nLine);
	void fOutputClass();
	void fReload();
    void fDelete();
    virtual ~ObjFile();
    std::string fGetMembers();
    void fReparse(int _nLine, int _nLastLine,const char* _sText);
	std::string fGetMember(const Scope* scope, int _nMinLine = 0);
	std::string fGetFunctions();
	std::string fGetImport(unsigned int _nIndex);
    void fReadBufferToToken( const char* _aBuff , unsigned int _nLenght);
	void fLoadAllInclude();
	std::string fGetAllFunctions();
	std::string fGetAllSubFile(std::vector<ObjFile*>& _aFiles);
    void fGetLocalScopeVar(int _nLine, int _nLastLine);

};

/// @}
//---------------------------------------------------------------------------
#endif // preprocessorH
