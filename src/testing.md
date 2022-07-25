# Testing

I worked on this project with TDD as an objective, but as usual when feeling my way on HOW to do stuff I flipped in and out of TDD into R&D mode and sometimes did TAD.

Without pointing fingers or making judgements, I have observed this seems to be the MO of most projects/developers where the developers evolve code with randomised pattern selections rather than design it as a team with well known and understood pattern uses. I have worked in both environments btw, and where no testing is done at all except by (unfortuante) humans.

I have diverged from strict TDD in order to use the Tests them selves as execution engine via Integration tests for R&D - see that it works and also how.

I am back filling as I go to isolate more code into unit tests.

I dislike this way of doing things because it loses the 'TDD as an act of design' effect.

# Notes on (my) CRAPpy code.

I engineered a free coverage solution using OpenCover and ReportGenerator packages from within the `coverage.bat` that I run from a terminal.

The most interesting discovery I amde about my code was that whilst I thought it was fairly decent, the initial reports did not. My [CRAPpY](https://testing.googleblog.com/2011/02/this-code-is-crap.html) scores were initialy way to high.

They made me revisit the code and do what I know I should have done anyway, which is to;
* Simplifiy the code.
* Dont give in to the temptation to just make it work and move on.




