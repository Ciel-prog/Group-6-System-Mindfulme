% MindfulMe Recommendation Knowledge Base

% DECLARE DYNAMIC PREDICATES - these will be set at runtime
:- dynamic stress_score/1.
:- dynamic mood_score/1.
:- dynamic sleep_hours/1.

% Facts about user states
stress_level(high) :- stress_score(S), S >= 8.
stress_level(medium) :- stress_score(S), S >= 5, S < 8.
stress_level(low) :- stress_score(S), S < 5.

mood_state(low) :- mood_score(M), M =< 3.
mood_state(medium) :- mood_score(M), M > 3, M =< 7.
mood_state(high) :- mood_score(M), M > 7.

sleep_quality(poor) :- sleep_hours(H), H < 6.
sleep_quality(fair) :- sleep_hours(H), H >= 6, H < 7.
sleep_quality(good) :- sleep_hours(H), H >= 7.

% Recommendation rules
recommend('Guided Meditation') :- stress_level(high), sleep_quality(poor).
recommend('Breathing Exercise') :- stress_level(high).
recommend('Sleep Story Audio') :- sleep_quality(poor).
recommend('Journal Prompts') :- mood_state(low).
recommend('Light Exercise') :- stress_level(medium).
recommend('Maintenance Practice') :- mood_state(high), stress_level(low), sleep_quality(good).
recommend('General Wellness').

% Default values removed - these will be set dynamically from C#
% stress_score(5). 
% mood_score(6).
% sleep_hours(7).