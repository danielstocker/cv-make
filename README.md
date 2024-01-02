# cv-make

CVMake allows you to create a customized resume out of different building blocks.
The tool uses a set of files as a source and builds out resume pages in HTML based off a template. 

## Features

- Create your resume template and customize it for each job that you apply to
- Feed in a job description 
  - The program assigns a score to each component of your resume based on the contents of the job description and removes/adds aspects based on it
- Define desired and minimum amount of bullets for a given element
  - The program will do its best not to cut content beyond the minimum amount
- Define various options for different text blocks
- Define sections based on your needs
- Export your resume as HTML pages that can then be converted into a PDF

## How to use

In its current version the tool definitely still needs some tinkering, but here are some instructions on how to utilize it. 

### Set your preferences

Upon the first run of the tool a settings file will be created. 
Here are the options that you can define in it. 

- inputJob 
  - points to a text file containing the job description
  - this defaults to the input.txt file in the "template" directory
- templatePath
  - this defaults to the "template" directory
- pageTemplate
  - a html file containing your resume template
  - any CSS files referenced will be detected automatically
  - if referencing other resources they may not be copied automatically (yet)
- targetPages
  - a number (defaults to 2) describing the hard limit of pages for your resume
- exportTo
  - points to the export directory. Defaults to the current working directory and "exports"
- targetSize
  - the length of the page - defaults to an American Letter size
  - note that depending on what browser you choose to view your resume in the length may appear/print differently
  - I've had good results with Chromium and Firefox 

### Build your resume template

Use HTML and CSS to build a page template. I've provided my template that you are welcome to use as a starting point. 
You will see in the template that you can call a dynamic section with the following syntax.

```
## SECTIONNAME
```

or

```
## ARRAY-SECTIONNAME
```

The tool will expect to find a *.txt file for each section in the input folder. The section name should be all lowercase and should not contain spaces. 
If using "array" the tool will expect one or more files with the section name and a number. (for example: job1.txt, job2.txt)

The text files used to create the dynamic sections use the following syntax:

- ##COLUMNS
  - should be followed by a line break and the number of columns to display this element in
  - the columns will be used for the list items. the intro element is not affected by this setting.
- ##INTRO
  - this element is optional and can contain one or more lines of HTML that appear ahead of the list items
- ##LISTITEMS
  - should be followed by a line break and the number of desired elements to display in the list.
  - the following options can also be added. each option should be placed on a separate line
    - strict - the number specified should only be reduced if all "non-strict" elements have been reduced as much as possible
    - min-<NUMBER> - the minimum number of bullets to display. If the tool reduces the bullets to this number then the "strict" behavior applies
    - droppable - the element can be removed all together if it doesn't fit
    - flexible - opposite of strict. reduce lines from this element first. 
    - no-bullet - do not display bullet points for the bullets
- ##ITEM
  - create one of these elements per item to display
  - each element can contain one or more lines of HTML that appear for the list item

### Create a resume

To start the customization process, ensure that your input file contains the job description of the job that you are applying for. 
Next run the tool. Note that Playwright will open a browser window to check the size of each page several times. 

Following the run you can find a HTML page for each page of the resume in the export folder. 
You are able to further adjust each page before exporting it into a different format and submitting it for your job application.