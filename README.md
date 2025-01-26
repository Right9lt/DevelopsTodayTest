# Introduction
This is a test assessment for the C#/.NET role at DevelopsToday
# Sql Queries
Query to create table is located [here](./SQLQuery.sql) 
# Number of rows
After running the program, there are 29779 rows in the table and 221 in the duplicates.csv file, which sums up to 30000 as it was in the original file. I removed all occurrences of duplicate rows before writing them into the database.
# Comments on assumptions 
To operate with large files, I would change the loading from CSV files to load data by chunks and process them by multiple threads.
